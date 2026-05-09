// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.IO.Compression;
using K4os.Compression.LZ4;
using Pluto.IO.Binary;
using SevenZip.Compression.LZMA;

namespace Charon.Compression;

public static class CompressionHelper {
	// could use mspack, but we'd have to implement our own io handlers.
	internal const string Lz4LibraryName = "lz4";
	internal const string LzxLibraryName = "chm";
	internal const string LzoLibraryName = "lzo2";
	internal const string OodleLibraryName = "oo2core";
	internal const string OodleTexLibraryName = "oo2texrt";
	internal const string ZstdLibraryName = "zstd";
	internal const string DensityLibraryName = "density";
	internal const string GDeflateLibraryName = "GDeflate";

	static CompressionHelper() => NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);

	public static bool EnableLogging { get; set; } = false;

	internal static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) {
		if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var handle)) {
			return handle;
		}

		var name = Path.GetFileNameWithoutExtension(libraryName);
		var cwd = AppDomain.CurrentDomain.BaseDirectory;

		string ext;
		if (OperatingSystem.IsWindows()) {
			ext = ".dll";
		} else if (OperatingSystem.IsLinux()) {
			ext = ".so";
		} else if (OperatingSystem.IsMacOS()) {
			ext = ".dylib";
		} else {
			return nint.Zero;
		}

		foreach (var dir in new[] { Path.Combine(cwd, $"runtimes/{RuntimeInformation.RuntimeIdentifier}/native/"), cwd }) {
			foreach (var libName in new[] { name, "lib" + name }) {
				var target = Path.Combine(dir, libName) + ext;
				if (!File.Exists(target)) {
					continue;
				}

				var ptr = NativeLibrary.Load(target);
				if (ptr != nint.Zero) {
					return ptr;
				}
			}
		}

		return nint.Zero;
	}

	internal static bool CanLoadLibrary(string libraryName) => NativeLibrary.TryLoad(libraryName, out _);

	public static bool CanDecompress(CompressionType compressionType) =>
		compressionType switch {
			CompressionType.None => true,
			CompressionType.Brotli => true,
			CompressionType.Zlib => true,
			CompressionType.Deflate => true,
			CompressionType.Gzip => true,
			CompressionType.LZ4 => true,
			CompressionType.LZ4HC => true,
			CompressionType.SizedLZMA => true,
			CompressionType.LZMA => true,
			CompressionType.HeaderlessLZMA => true,
			CompressionType.Oodle => CanLoadLibrary(OodleLibraryName),
			CompressionType.OodleTex => CanLoadLibrary(OodleTexLibraryName),
			CompressionType.LZ4F => CanLoadLibrary(Lz4LibraryName),
			CompressionType.LZO1 or CompressionType.LZO2 => CanLoadLibrary(LzoLibraryName),
			CompressionType.LZX => CanLoadLibrary(LzxLibraryName),
			CompressionType.Zstd => CanLoadLibrary(ZstdLibraryName),
			CompressionType.Density => CanLoadLibrary(DensityLibraryName),
			CompressionType.GDeflate => CanLoadLibrary(GDeflateLibraryName),
			_ => false,
		};
	public static bool CanCompress(CompressionType compressionType) =>
		compressionType switch {
			CompressionType.None => true,
			CompressionType.Brotli => true,
			CompressionType.Zlib => true,
			CompressionType.Deflate => true,
			CompressionType.Gzip => true,
			CompressionType.LZ4 => true,
			CompressionType.LZ4HC => true,
			CompressionType.Oodle => CanLoadLibrary(OodleLibraryName),
			CompressionType.Zstd => CanLoadLibrary(ZstdLibraryName),
			CompressionType.GDeflate => CanLoadLibrary(GDeflateLibraryName),
			_ => false,
		};

	public static int FindDecompressedStreamLength(Stream stream, int bufSize = 81920, bool leaveOpen = false) {
		var buf = new RentedArray<byte>(bufSize);
		var length = 0L;
		var buffer = buf.Span;

		try {
			int read;
			while((read = stream.Read(buffer)) > 0) {
				length += read;
			}
		} finally {
			if (!leaveOpen) {
				stream.Dispose();
			}
		}

		return checked((int) length);
	}

	public static unsafe int FindDecompressedStreamLength(CompressionType type, Memory<byte> compressed, int bufSize = 81920, bool leaveOpen = false) {
		using var dataPin = compressed.Pin();
		using var dataStream = new UnmanagedMemoryStream((byte*) dataPin.Pointer, compressed.Length);
		return type switch {
			CompressionType.Zlib => FindDecompressedStreamLength(new ZLibStream(dataStream, CompressionMode.Decompress)),
			CompressionType.Deflate => FindDecompressedStreamLength(new DeflateStream(dataStream, CompressionMode.Decompress)),
			CompressionType.Gzip => FindDecompressedStreamLength(new GZipStream(dataStream, CompressionMode.Decompress)),
			CompressionType.Brotli => FindDecompressedStreamLength(new BrotliStream(dataStream, CompressionMode.Decompress)),
			_ => 0
		};
	}

	public static int FindDecompressedLength(CompressionType type, Memory<byte> compressed, bool hasJunk = false) {
		return type switch {
			CompressionType.Zlib or CompressionType.Deflate or CompressionType.Brotli => FindDecompressedStreamLength(type, compressed),
			CompressionType.Gzip when hasJunk => FindDecompressedStreamLength(type, compressed),
			CompressionType.Gzip => MemoryMarshal.Read<int>(compressed.Span[^4..]),
			CompressionType.SizedLZMA or CompressionType.LZMA => (int) MemoryMarshal.Read<long>(compressed.Span[5..]),
			CompressionType.Zstd => ZStandard.GetDecompressBound(compressed),
			_ => -1,
		};
	}

	public static unsafe int Decompress(CompressionType type, Memory<byte> compressed, Memory<byte> decompressed) {
		switch (type) {
			case CompressionType.Zlib: {
				using var dataPin = compressed.Pin();
				using var dataStream = new UnmanagedMemoryStream((byte*) dataPin.Pointer, compressed.Length);
				using var zlib = new ZLibStream(dataStream, CompressionMode.Decompress);
				zlib.ReadExactly(decompressed.Span);
				return decompressed.Length;
			}
			case CompressionType.Deflate: {
				using var dataPin = compressed.Pin();
				using var dataStream = new UnmanagedMemoryStream((byte*) dataPin.Pointer, compressed.Length);
				using var deflate = new DeflateStream(dataStream, CompressionMode.Decompress);
				deflate.ReadExactly(decompressed.Span);
				return decompressed.Length;
			}
			case CompressionType.Zstd: {
				using var zstd = new ZStandard();
				return zstd.Decompress(compressed, decompressed);
			}
			case CompressionType.Gzip: {
				using var dataPin = compressed.Pin();
				using var dataStream = new UnmanagedMemoryStream((byte*) dataPin.Pointer, compressed.Length);
				using var gzip = new GZipStream(dataStream, CompressionMode.Decompress);
				gzip.ReadExactly(decompressed.Span);
				return decompressed.Length;
			}
			case CompressionType.Oodle: {
				return Oodle.Decompress(compressed, decompressed);
			}
			case CompressionType.OodleTex: {
				return OodleTex.Decompress(compressed, decompressed);
			}
			case CompressionType.LZ4:
			case CompressionType.LZ4HC: {
				return LZ4Codec.Decode(compressed.Span, decompressed.Span);
			}
			case CompressionType.LZ4F: {
				return Lz4Frame.Decompress(compressed, decompressed);
			}
			case CompressionType.Brotli: {
				using var dataPin = compressed.Pin();
				using var dataStream = new UnmanagedMemoryStream((byte*) dataPin.Pointer, compressed.Length);
				using var brotli = new BrotliStream(dataStream, CompressionMode.Decompress);
				brotli.ReadExactly(decompressed.Span);
				return decompressed.Length;
			}
			case CompressionType.LZO1: {
				return LZO.DecompressLzo1(compressed, decompressed);
			}
			case CompressionType.LZO2: {
				return LZO.DecompressLzo2(compressed, decompressed);
			}
			case CompressionType.LZX: {
				return LZX.Decompress(compressed, decompressed, 17);
			}
			case CompressionType.SizedLZMA:
			case CompressionType.LZMA:
			case CompressionType.HeaderlessLZMA: {
				using var inPin = compressed.Pin();
				using var inStream = new UnmanagedMemoryStream((byte*) inPin.Pointer, compressed.Length, compressed.Length, FileAccess.Read);
				using var outPin = decompressed.Pin();
				using var outStream = new UnmanagedMemoryStream((byte*) outPin.Pointer, decompressed.Length, decompressed.Length, FileAccess.ReadWrite);
				var array = ArrayPool<byte>.Shared.Rent(5);
				try {
					var coder = new Decoder();
					compressed[..5].CopyTo(array);
					coder.SetDecoderProperties(array[..5]);
					inStream.Position = 5;
					// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
					switch (type) {
						case CompressionType.SizedLZMA:
							inStream.Position += 16; // skip uncompressedSize, compressedSize
							break;
						case CompressionType.LZMA:
							inStream.Position += 8; // skip uncompressedSize
							break;
					}

					coder.Code(inStream, outStream, inStream.Length - inStream.Position, outStream.Length, default);
					outStream.Flush();
				} finally {
					ArrayPool<byte>.Shared.Return(array);
				}

				return (int) outStream.Length;
			}
			case CompressionType.Density: {
				return Density.Decompress(compressed, decompressed);
			}
			case CompressionType.GDeflate: {
				return GDeflate.Decompress(compressed, decompressed);
			}
			case CompressionType.None:
				compressed.CopyTo(decompressed);
				return decompressed.Length;
			default:
				throw new NotSupportedException("Compression type is not supported");
		}
	}

	public static unsafe int Compress(CompressionType type, Memory<byte> compressed, Memory<byte> decompressed, CompressionLevel compressionLevel = CompressionLevel.Fastest) {
		// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
		switch (type) {
			case CompressionType.Zlib: {
				using var dataPin = decompressed.Pin();
				using var dataStream = new UnmanagedMemoryStream((byte*) dataPin.Pointer, decompressed.Length);
				using var zlib = new ZLibStream(dataStream, compressionLevel);
				zlib.Write(compressed.Span);
				zlib.Flush();
				return (int) zlib.Position;
			}
			case CompressionType.Deflate: {
				using var dataPin = decompressed.Pin();
				using var dataStream = new UnmanagedMemoryStream((byte*) dataPin.Pointer, decompressed.Length);
				using var deflate = new DeflateStream(dataStream, compressionLevel);
				deflate.Write(compressed.Span);
				deflate.Flush();
				return (int) deflate.Position;
			}
			case CompressionType.Zstd: {
				using var zstd = new ZStandard();
				return (int) zstd.Compress(decompressed, compressed,
					compressionLevel switch {
						CompressionLevel.Optimal => ZSTDCompressionLevel.BTOptimal,
						CompressionLevel.Fastest => ZSTDCompressionLevel.DecompressFast,
						CompressionLevel.NoCompression => ZSTDCompressionLevel.None,
						CompressionLevel.SmallestSize => ZSTDCompressionLevel.BTVeryUltra,
						_ => throw new ArgumentOutOfRangeException(nameof(compressionLevel), compressionLevel, default),
					});
			}
			case CompressionType.Gzip: {
				using var dataPin = compressed.Pin();
				using var dataStream = new UnmanagedMemoryStream((byte*) dataPin.Pointer, compressed.Length);
				dataStream.Position = 2;
				using var gzip = new GZipStream(dataStream, compressionLevel);
				gzip.Write(decompressed.Span);
				gzip.Flush();
				return (int) gzip.Position;
			}
			case CompressionType.Oodle: {
				return Oodle.Compress(decompressed, compressed, Oodle.OodleLZ_Compressor.Hydra,
					compressionLevel switch {
						CompressionLevel.Optimal => Oodle.OodleLZ_CompressionLevel.Optimal,
						CompressionLevel.Fastest => Oodle.OodleLZ_CompressionLevel.Min,
						CompressionLevel.NoCompression => Oodle.OodleLZ_CompressionLevel.None,
						CompressionLevel.SmallestSize => Oodle.OodleLZ_CompressionLevel.Max,
						_ => throw new ArgumentOutOfRangeException(nameof(compressionLevel), compressionLevel, default),
					});
			}
			case CompressionType.LZ4:
			case CompressionType.LZ4HC:
				return LZ4Codec.Encode(decompressed.Span, compressed.Span);
			case CompressionType.Brotli: {
				using var dataPin = compressed.Pin();
				using var dataStream = new UnmanagedMemoryStream((byte*) dataPin.Pointer, compressed.Length);
				using var brotli = new BrotliStream(dataStream, compressionLevel);
				brotli.Write(decompressed.Span);
				brotli.Flush();
				return (int) brotli.Position;
			}
			case CompressionType.GDeflate:
				return GDeflate.Compress(decompressed, compressed, 12);
			case CompressionType.None:
				decompressed.CopyTo(compressed);
				return decompressed.Length;
			default:
				throw new NotSupportedException("Compression type is not supported");
		}
	}
}
