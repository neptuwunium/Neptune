// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Triton.Pixel.Formats;

namespace Triton.Encoder;

public partial class PNGEncoder(PNGCompressionLevel compressionLevel) : IEncoder {
	static PNGEncoder() {
		PNGVersion = "1.6.0";

		if (!NativeLibrary.TryLoad(NativeMethods.LibraryName, Assembly.GetExecutingAssembly(), NativeMethods.SearchPath, out var ptr)) {
			return;
		}

		NativeLibrary.Free(ptr);
		IsAvailable = true;
		var pngPtr = NativeMethods.png_get_libpng_ver(nint.Zero);
		if (pngPtr != nint.Zero) {
			PNGVersion = Marshal.PtrToStringAnsi(pngPtr) ?? PNGVersion;
		}
	}

	public PNGCompressionLevel CompressionLevel { get; set; } = compressionLevel;
	public static bool IsAvailable { get; }
	public static string PNGVersion { get; }

	public void Write(Stream stream, EncoderWriteOptions options, ImageCollection image) => Write(stream, options, image[0]);

	public unsafe ImageCollection Read(Stream stream) {
		var png = NativeMethods.png_create_read_struct(PNGVersion, nint.Zero, nint.Zero, nint.Zero);
		if (png == nint.Zero) {
			throw new OutOfMemoryException();
		}

		var info = nint.Zero;

		try {
			info = NativeMethods.png_create_info_struct(png);
			if (info == nint.Zero) {
				throw new OutOfMemoryException();
			}

			NativeMethods.png_set_read_fn(png, nint.Zero, ReadStream);

			if (!NativeMethods.png_get_IHDR(png, info, out var width, out var height, out var bitDepth, out var colorType, out _, out _, out _)) {
				throw new NotSupportedException();
			}

			if (colorType == PNGColorType.PaletteColor) {
				if (bitDepth == 16) {
					NativeMethods.png_set_expand_16(png);
				} else {
					NativeMethods.png_set_expand(png);
				}

				NativeMethods.png_read_update_info(png, info);

				if (!NativeMethods.png_get_IHDR(png, info, out width, out height, out bitDepth, out colorType, out _, out _, out _)) {
					throw new NotSupportedException();
				}
			}

			var samples = colorType switch {
				PNGColorType.Gray => 1,
				PNGColorType.GrayAlpha => 2,
				PNGColorType.RGB => 3,
				PNGColorType.RGBA => 4,
				PNGColorType.Palette => throw new NotSupportedException(),
				PNGColorType.PaletteColor => throw new NotSupportedException(),
				_ => throw new NotSupportedException(),
			};

			var image = IImageBuffer.Create(width, height, bitDepth, samples, false, false);

			NativeMethods.png_read_png(png, info, PNGTransform.SwapEndian, nint.Zero);
			var rowSize = (int) NativeMethods.png_get_rowbytes(png, info);
			var rows = NativeMethods.png_get_rows(png, info);
			var rowData = image.Data.Memory.Span;
			for (var rowIndex = 0; rowIndex < height; rowIndex++) {
				var row = new Span<byte>(rows[rowIndex], rowSize);
				row.CopyTo(rowData[(rowIndex * image.Width * image.Stride)..]);
			}

			GC.KeepAlive(stream);
			return [image];
		} finally {
			NativeMethods.png_destroy_read_struct(ref png, ref info, ref Unsafe.NullRef<nint>());
		}

		void ReadStream(nint _, nint dataPtr, nint dataSize) {
			var span = new Span<byte>((byte*) dataPtr, int.CreateChecked(dataSize));
			stream.ReadExactly(span);
		}
	}

	public void Write(Stream stream, EncoderWriteOptions options, IImageBuffer image) {
		if (image.ColorId.IsHDR) {
			using var image16 = image.ColorId.IsSigned ? image.Cast<short>() : image.Cast<ushort>();
			WriteCore(stream, options, image16);
			return;
		}

		WriteCore(stream, options, image);
	}

	public unsafe void WriteCore(Stream stream, EncoderWriteOptions options, IImageBuffer image) {
		if (image.ColorId.IsHDR || image.ColorId.Components is not (>= 1 and <= 4)) {
			throw new NotSupportedException();
		}

		var png = NativeMethods.png_create_write_struct(PNGVersion, nint.Zero, nint.Zero, nint.Zero);
		if (png == nint.Zero) {
			throw new OutOfMemoryException();
		}

		var info = nint.Zero;
		try {
			info = NativeMethods.png_create_info_struct(png);
			if (info == nint.Zero) {
				throw new OutOfMemoryException();
			}

			NativeMethods.png_set_write_fn(png, nint.Zero, WriteStream, FlushStream);
			NativeMethods.png_set_compression_level(png, options.Compress ? CompressionLevel : 0);
			var colorType = image.ColorId.Components switch { 1 => PNGColorType.Gray, 2 => PNGColorType.GrayAlpha, 3 => PNGColorType.RGB, 4 => PNGColorType.RGBA, _ => throw new UnreachableException() };
			NativeMethods.png_set_IHDR(png, info, image.Width, image.Height, image.ColorId.Bits, colorType, PNGInterlacing.None, PNGCompressionType.Default, PNGFilterType.None);
			NativeMethods.png_write_info(png, info);

			var rowData = image.Data.Memory;

			using var rowPin = rowData.Pin();

			var rows = stackalloc byte*[image.Height];
			for (var rowIndex = 0; rowIndex < image.Height; rowIndex++) {
				rows[rowIndex] = (byte*) rowPin.Pointer + rowIndex * image.Width * image.Stride;
			}

			NativeMethods.png_set_rows(png, info, rows);
			NativeMethods.png_write_png(png, info, PNGTransform.SwapEndian, nint.Zero);
			NativeMethods.png_write_end(png, info);

			GC.KeepAlive(rowPin);
		} finally {
			NativeMethods.png_destroy_write_struct(ref png, ref info);
		}

		GC.KeepAlive(stream);
		return;

		void WriteStream(nint _, nint dataPtr, nint dataSize) {
			stream.Flush();
			var span = new Span<byte>((byte*) dataPtr, int.CreateChecked(dataSize));
			stream.Write(span);
		}

		void FlushStream(nint _) => stream.Flush();
	}

	[Flags, SuppressMessage("ReSharper", "InconsistentNaming")]
	internal enum PNGColorType {
		Gray = 0,
		Palette = 1,
		Color = 2,
		Alpha = 4,

		PaletteColor = Color | Palette,
		RGB = Color,
		RGBA = Color | Alpha,
		GrayAlpha = Gray | Alpha,
	}

	internal enum PNGCompressionType {
		Deflate = 0,

		Default = Deflate,
	}

	internal enum PNGFilterType {
		None = 0,
		Differencing = 64,

		Default = None,
	}

	internal enum PNGInterlacing {
		None = 0,
		Adam7 = 1,
	}

	[Flags, SuppressMessage("ReSharper", "InconsistentNaming")]
	internal enum PNGTransform : uint {
		Identity = 0x0000,
		Strip16 = 0x0001,
		StripAlpha = 0x0002,
		Packing = 0x0004,
		PackSwap = 0x0008,
		Expand = 0x0010,
		InvertMono = 0x0020,
		Shift = 0x0040,
		BGR = 0x0080,
		SwapAlpha = 0x0100,
		SwapEndian = 0x0200,
		InvertAlpha = 0x0400,
		StripFiller = 0x0800,
		StripFillerAfter = 0x1000,
		GrayscaleToRgb = 0x2000,
		Expand16 = 0x4000,
		Scale16 = 0x8000,
	}

	[StructLayout(LayoutKind.Sequential)]
	internal record struct PngColor16 {
		public byte Index { get; set; }
		public ColorRGBA<ushort> Color { get; set; }
	}

	private static partial class NativeMethods {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void PNGFlush(nint png);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void PNGReadWrite(nint png, nint ptr, nint size);

		internal const string LibraryName = "png";
		internal const DllImportSearchPath SearchPath = DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory | DllImportSearchPath.ApplicationDirectory;

		[LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8), DefaultDllImportSearchPaths(SearchPath)]
		public static partial nint png_create_write_struct([MarshalAs(UnmanagedType.LPStr)] string userPNGVersion, nint errorPtr, nint errorFunc, nint warnFunc);

		[LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8), DefaultDllImportSearchPaths(SearchPath)]
		public static partial nint png_create_read_struct([MarshalAs(UnmanagedType.LPStr)] string userPNGVersion, nint errorPtr, nint errorFunc, nint warnFunc);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial nint png_create_info_struct(nint pngPtr);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_destroy_write_struct(ref nint pngPtr, ref nint infoPtr);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_destroy_read_struct(ref nint pngPtr, ref nint infoPtr, ref nint infoEndPtr);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_set_write_fn(nint pngPtr, nint ioPtr, [MarshalAs(UnmanagedType.FunctionPtr)] PNGReadWrite? write, [MarshalAs(UnmanagedType.FunctionPtr)] PNGFlush? flush);


		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_set_read_fn(nint pngPtr, nint ioPtr, [MarshalAs(UnmanagedType.FunctionPtr)] PNGReadWrite? write);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_set_IHDR(nint pngPtr, nint infoPtr, int width, int height, int bitDepth, PNGColorType colorType, PNGInterlacing interlaceMethod, PNGCompressionType compressionMethod, PNGFilterType filterMethod);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		[return: MarshalAs(UnmanagedType.I4)]
		public static partial bool png_get_IHDR(nint pngPtr, nint infoPtr, out int width, out int height, out int bitDepth, out PNGColorType colorType, out PNGInterlacing interlaceMethod, out PNGCompressionType compressionMethod, out PNGFilterType filterMethod);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_set_expand(nint pngPtr);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_set_expand_16(nint pngPtr);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_read_update_info(nint pngPtr, nint infoPtr);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_write_info(nint pngPtr, nint infoPtr);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static unsafe partial void png_set_rows(nint pngPtr, nint infoPtr, byte** rowPtr);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static unsafe partial long png_get_rowbytes(nint pngPtr, nint infoPtr);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static unsafe partial byte** png_get_rows(nint pngPtr, nint infoPtr);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_write_end(nint pngPtr, nint infoPtr);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_set_compression_level(nint pngPtr, PNGCompressionLevel level);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial nint png_get_libpng_ver(nint pngPtr); // for some reason string doesn't work here

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_write_png(nint pngPtr, nint infoPtr, PNGTransform transforms, nint @params);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void png_read_png(nint pngPtr, nint infoPtr, PNGTransform transforms, nint @params);
	}
}
