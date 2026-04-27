// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Compression;

public static partial class GDeflate {
	public enum TileStreamCompressor : byte {
		GDeflate = 4,
	}

	public const int TileSize = 0x10000;
	public const int MaxTiles = 0xFFFF;
	public const int TileHeaderSize = sizeof(uint) + 4 * 208 + 4 * 8;
	public const int FullTileSize = TileSize + TileHeaderSize;

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
	public record struct TileStreamHeader() {
		public TileStreamCompressor Id {
			get;
			set {
				field = value;
				Magic = (byte) (0xFF ^ (byte) value);
			}
		}

		public byte Magic { get; private set; }
		public ushort NumTiles { get; set; }
		public uint Flags { get; set; } = 1;

		public int TileSizeIndex {
			get => (int) (Flags & 3);
			set => Flags = (Flags & 0xFFFFFFFCU) | ((uint) value & 3);
		}

		public int LastTileSize {
			get => (int) ((Flags >> 2) & 0x3FFFFU);
			set => Flags = (Flags & 0xFFF00003U) | (((uint) value & 0x3FFFFU) << 2);
		}

		public int Reserved {
			get => (int) (Flags >> 20);
			set => Flags = (Flags & 0xFFFFFU) | (((uint) value & 0xFFFFFU) << 20);
		}

		public bool Valid => (byte) Id == (0xFF ^ Magic);

		public int UncompressedSize {
			get => NumTiles * TileSize - (LastTileSize == 0 ? 0 : TileSize - LastTileSize);
			set {
				NumTiles = (ushort) (value / TileSize);
				LastTileSize = value - NumTiles * TileSize;
				if (LastTileSize > 0) {
					NumTiles++;
				}
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	// ReSharper disable twice NotAccessedPositionalProperty.Local
	private record struct GDeflatePage(nint Data, int Size);

	private static partial class NativeMethods {
		[LibraryImport(CompressionHelper.GDeflateLibraryName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
		internal static partial nint libdeflate_alloc_gdeflate_compressor(int level);

		[LibraryImport(CompressionHelper.GDeflateLibraryName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
		internal static unsafe partial nint libdeflate_gdeflate_compress(nint compressor, nint src, nint srcSize, GDeflatePage* pages, nint numPages);

		[LibraryImport(CompressionHelper.GDeflateLibraryName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
		internal static partial void libdeflate_free_gdeflate_compressor(nint compressor);

		[LibraryImport(CompressionHelper.GDeflateLibraryName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
		internal static partial nint libdeflate_alloc_gdeflate_decompressor();

		[LibraryImport(CompressionHelper.GDeflateLibraryName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
		internal static unsafe partial int libdeflate_gdeflate_decompress(nint compressor, GDeflatePage* pages, nint numPages, nint dst, nint dstSize, out nint bytes);

		[LibraryImport(CompressionHelper.GDeflateLibraryName), DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
		internal static partial void libdeflate_free_gdeflate_decompressor(nint compressor);
	}

	public static unsafe int Compress(ReadOnlyMemory<byte> uncompressed, Memory<byte> pool, int level) {
		var tileHeader = new TileStreamHeader {
			Id = TileStreamCompressor.GDeflate,
			NumTiles = (ushort) Math.Clamp((uncompressed.Length + TileSize - 1) / TileSize, 1, MaxTiles),
			LastTileSize = uncompressed.Length % TileSize,
		};
		var offset = Unsafe.SizeOf<TileStreamHeader>() + (tileHeader.NumTiles << 2);
		var size = Unsafe.SizeOf<TileStreamHeader>() + offset + (uncompressed.Length << 1);
		if (size > pool.Length) {
			return -1;
		}

		var compressed = pool;
		var outputSpan = pool.Span;
		MemoryMarshal.Write(outputSpan, tileHeader);

		var tileOffsets = MemoryMarshal.Cast<byte, int>(outputSpan[Unsafe.SizeOf<TileStreamHeader>()..])[..tileHeader.NumTiles];
		var compressedOffset = 0;
		var uncompressedOffset = 0;

		using var uncompressedPin = uncompressed.Pin();
		using var compressedPin = compressed.Pin();

		var compressor = NativeMethods.libdeflate_alloc_gdeflate_compressor(Math.Clamp(level, 1, 12));
		var page = stackalloc GDeflatePage[1];
		try {
			for (var tileIndex = 0; tileIndex < tileHeader.NumTiles; tileIndex++) {
				var slice = uncompressed[uncompressedOffset..];
				if (slice.Length > TileSize) {
					slice = slice[..TileSize];
				}

				var uncompressedPtr = (nint) uncompressedPin.Pointer + uncompressedOffset;
				uncompressedOffset += slice.Length;

				var outputSlice = compressed[(compressedOffset + offset)..];

				// it could in theory just pass a big list of pages, but it'd have giant padding blocks everywhere that would have to be removed.
				// this ends up being uglier but more performance as it omits several memcpy operations.
				page[0] = new GDeflatePage((nint) compressedPin.Pointer + (compressedOffset + offset), outputSlice.Length);
				var compressedSize = NativeMethods.libdeflate_gdeflate_compress(compressor, uncompressedPtr, slice.Length, page, 1);
				if (compressedSize == 0) {
					size = 0;
					break;
				}

				compressedOffset += (int) compressedSize;
				if (tileIndex < tileHeader.NumTiles - 1) {
					tileOffsets[tileIndex + 1] = compressedOffset;
				} else {
					tileOffsets[0] = (int) compressedSize;
					var newSize = (int) (compressedOffset + offset + compressedSize);
					if (newSize > size) {
						throw new IndexOutOfRangeException(); // shouldn't happen!
					}

					size = newSize;
				}
			}

			return size;
		} finally {
			NativeMethods.libdeflate_free_gdeflate_compressor(compressor);
		}
	}

	public static unsafe int Decompress(ReadOnlyMemory<byte> compressed, Memory<byte> uncompressed) {
		uncompressed.Span.Clear();
		var compressedSpan = compressed.Span;
		var tileHeader = MemoryMarshal.Read<TileStreamHeader>(compressedSpan);
		if (!tileHeader.Valid) {
			return -1;
		}

		if (tileHeader.Id != TileStreamCompressor.GDeflate) {
			return -1;
		}

		var tileOffsets = MemoryMarshal.Cast<byte, int>(compressedSpan[Unsafe.SizeOf<TileStreamHeader>()..])[..tileHeader.NumTiles];
		var offset = Unsafe.SizeOf<TileStreamHeader>() + (tileHeader.NumTiles << 2);
		var pages = stackalloc GDeflatePage[tileHeader.NumTiles];
		var safePages = new Span<GDeflatePage>(pages, tileHeader.NumTiles);
		using var compressedPin = compressed.Pin();

		for (var tileIndex = 0; tileIndex < tileHeader.NumTiles; tileIndex++) {
			var tileOffset = tileIndex > 0 ? tileOffsets[tileIndex] : 0;
			var tileSize = tileIndex < tileHeader.NumTiles - 1 ? tileOffsets[tileIndex + 1] - tileOffset : tileOffsets[0];
			_ = compressed.Slice(offset, tileSize); // this is bounds checking
			safePages[tileIndex] = new GDeflatePage((nint) compressedPin.Pointer + offset, tileSize);
			offset += tileSize;
		}

		using var decompressedPin = uncompressed.Pin();
		var decompressor = NativeMethods.libdeflate_alloc_gdeflate_decompressor();
		try {
			var result = NativeMethods.libdeflate_gdeflate_decompress(decompressor, pages, tileHeader.NumTiles, (nint) decompressedPin.Pointer, uncompressed.Length, out var bytes);
			return result == 0 ? (int) bytes : 0;
		} finally {
			NativeMethods.libdeflate_free_gdeflate_decompressor(decompressor);
		}
	}
}
