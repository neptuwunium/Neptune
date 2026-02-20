// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2 OR LGPL-3.0-or-later
// You may choose either license when using or modifying this code.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Triton.Encoder;

public partial class TIFFEncoder : IEncoder {
	static TIFFEncoder() {
		NativeHelper.Register();

		var ptr = NativeHelper.DllImportResolver(NativeMethods.LibraryName, Assembly.GetExecutingAssembly(), NativeMethods.SearchPath);
		if (ptr == nint.Zero) {
			return;
		}

		IsAvailable = true;
		NativeLibrary.Free(ptr);
	}

	public TIFFEncoder(TIFFCompression compression, TIFFCompression hdrCompression) {
		Compression = compression;
		HDRCompression = hdrCompression;
	}

	public TIFFCompression Compression { get; set; }
	public TIFFCompression HDRCompression { get; set; }

	public static bool IsAvailable { get; }

	public unsafe void Write(Stream stream, EncoderWriteOptions options, ImageCollection frames) {
		var readProc = Marshal.GetFunctionPointerForDelegate((NativeMethods.TIFFReadWriteProc) ReadProc);
		var writeProc = Marshal.GetFunctionPointerForDelegate((NativeMethods.TIFFReadWriteProc) WriteProc);
		var seekProc = Marshal.GetFunctionPointerForDelegate((NativeMethods.TIFFSeekProc) SeekProc);
		var closeProc = Marshal.GetFunctionPointerForDelegate((NativeMethods.TIFFCloseProc) CloseProc);
		var sizeProc = Marshal.GetFunctionPointerForDelegate((NativeMethods.TIFFSizeProc) SizeProc);
		var tiff = NativeMethods.TIFFClientOpen(stream is FileStream fs ? Path.GetFileName(fs.Name) : "TritonImage", "w", nint.Zero,
			readProc, writeProc, seekProc, closeProc, sizeProc, nint.Zero, nint.Zero);
		if (tiff == nint.Zero) {
			throw new OutOfMemoryException();
		}

		try {
			var extraSamples = stackalloc ushort[1];
			extraSamples[0] = (ushort) (options.AssociateAlpha ? TIFFExtraSamples.AssociatedAlpha : TIFFExtraSamples.UnassociatedAlpha);

			foreach (var frame in frames) {
				var frameMut = frame;

				IImageBuffer? image = null;
				if (frameMut.ColorId.Layout != ChannelLayout.RedFirst) {
					image = frameMut.Cast(frameMut.ColorId.Components);
					frameMut = image;
				}

				NativeMethods.TIFFSetField(tiff, TIFFTag.ImageWidth, frameMut.Width);
				NativeMethods.TIFFSetField(tiff, TIFFTag.ImageLength, frameMut.Height);
				NativeMethods.TIFFSetField(tiff, TIFFTag.RowsPerStrip, frameMut.Height);
				NativeMethods.TIFFSetField(tiff, TIFFTag.SamplesPerPixel, frameMut.ColorId.Components);
				NativeMethods.TIFFSetField(tiff, TIFFTag.BitsPerSample, frameMut.ColorId.Bits);
				NativeMethods.TIFFSetField(tiff, TIFFTag.SampleFormat, (int) (frameMut.ColorId.IsHDR ? TIFFSampleFormat.Float : frameMut.ColorId.IsSigned ? TIFFSampleFormat.Int : TIFFSampleFormat.UInt));
				NativeMethods.TIFFSetField(tiff, TIFFTag.Orientation, (int) TIFFOrientation.TopLeft);
				NativeMethods.TIFFSetField(tiff, TIFFTag.PlanarConfig, (int) TIFFPlanarConfig.Contig);
				NativeMethods.TIFFSetField(tiff, TIFFTag.Photometric, (int) (frameMut.ColorId.Components < 3 ? TIFFPhotometric.MinIsBlack : TIFFPhotometric.RGB));
				NativeMethods.TIFFSetField(tiff, TIFFTag.Compression, (int) (!options.Compress ? TIFFCompression.None : frameMut.ColorId.IsHDR ? HDRCompression : Compression));
				if (frameMut.ColorId.Components is 2 or 4) {
					NativeMethods.TIFFSetFieldArray(tiff, TIFFTag.ExtraSamples, 1, (nint) extraSamples);
				}

				try {
					var rowData = frameMut.Data.Memory;
					using var rowPin = rowData.Pin();
					NativeMethods.TIFFWriteEncodedStrip(tiff, 0, (nint) rowPin.Pointer, (uint) (frameMut.Width * frameMut.Height * frameMut.Stride));
					NativeMethods.TIFFWriteDirectory(tiff);
					GC.KeepAlive(rowPin);
				} finally {
					image?.Dispose();
				}
			}
		} finally {
			NativeMethods.TIFFClose(tiff);
		}

		GC.KeepAlive((NativeMethods.TIFFReadWriteProc) ReadProc);
		GC.KeepAlive((NativeMethods.TIFFReadWriteProc) WriteProc);
		GC.KeepAlive((NativeMethods.TIFFSeekProc) SeekProc);
		GC.KeepAlive((NativeMethods.TIFFCloseProc) CloseProc);
		GC.KeepAlive((NativeMethods.TIFFSizeProc) SizeProc);
		GC.KeepAlive(stream);
		return;

		nint ReadProc(nint _, nint dataPtr, nint dataSize) {
			var span = new Span<byte>((byte*) dataPtr, int.CreateChecked(dataSize));
			return stream.Read(span);
		}

		nint WriteProc(nint _, nint dataPtr, nint dataSize) {
			stream.Flush();
			var span = new Span<byte>((byte*) dataPtr, int.CreateChecked(dataSize));
			stream.Write(span);
			return dataSize;
		}

		ulong SeekProc(nint _, ulong offset, int whence) {
			var off = long.CreateChecked(offset);
			if (whence == 2) {
				off = -off;
			}

			return (ulong) stream.Seek(off, (SeekOrigin) whence);
		}

		int CloseProc(nint _) {
			stream.Flush();
			stream.Close();
			return 0;
		}

		ulong SizeProc(nint _) => (ulong) stream.Length;
	}

	public unsafe ImageCollection Read(Stream stream) {
		var readProc = Marshal.GetFunctionPointerForDelegate((NativeMethods.TIFFReadWriteProc) ReadProc);
		var writeProc = Marshal.GetFunctionPointerForDelegate((NativeMethods.TIFFReadWriteProc) WriteProc);
		var seekProc = Marshal.GetFunctionPointerForDelegate((NativeMethods.TIFFSeekProc) SeekProc);
		var closeProc = Marshal.GetFunctionPointerForDelegate((NativeMethods.TIFFCloseProc) CloseProc);
		var sizeProc = Marshal.GetFunctionPointerForDelegate((NativeMethods.TIFFSizeProc) SizeProc);
		var tiff = NativeMethods.TIFFClientOpen(stream is FileStream fs ? Path.GetFileName(fs.Name) : "TritonImage", "r", nint.Zero,
			readProc, writeProc, seekProc, closeProc, sizeProc, nint.Zero, nint.Zero);
		if (tiff == nint.Zero) {
			throw new OutOfMemoryException();
		}

		try {
			var numberOfFrames = NativeMethods.TIFFNumberOfDirectories(tiff);
			var frames = new ImageCollection(numberOfFrames);
			for (var frameIndex = 0; frameIndex < numberOfFrames; ++frameIndex) {
				NativeMethods.TiffGetField(tiff, TIFFTag.ImageWidth, out var width);
				NativeMethods.TiffGetField(tiff, TIFFTag.ImageLength, out var height);
				NativeMethods.TiffGetField(tiff, TIFFTag.RowsPerStrip, out var strip);
				NativeMethods.TiffGetField(tiff, TIFFTag.SamplesPerPixel, out var samples);
				NativeMethods.TiffGetField(tiff, TIFFTag.BitsPerSample, out var bitDepth);
				NativeMethods.TiffGetField(tiff, TIFFTag.SampleFormat, out var sampleFormat);

				if (strip <= 0) {
					strip = 1;
				}

				var image = IImageBuffer.Create(width, height, bitDepth, samples, (TIFFSampleFormat) sampleFormat == TIFFSampleFormat.Float, (TIFFSampleFormat) sampleFormat == TIFFSampleFormat.Int);
				using var rowData = image.Data.Memory.Pin();
				var stripSize = (uint) (width * strip * image.Stride);
				for (var rowIndex = 0; rowIndex < height; rowIndex += strip) {
					NativeMethods.TIFFReadEncodedStrip(tiff, rowIndex, (nint) rowData.Pointer + width * rowIndex * image.Stride, stripSize);
				}

				frames.Add(image);

				if (!NativeMethods.TIFFReadDirectory(tiff)) {
					break;
				}
			}

			GC.KeepAlive((NativeMethods.TIFFReadWriteProc) ReadProc);
			GC.KeepAlive((NativeMethods.TIFFReadWriteProc) WriteProc);
			GC.KeepAlive((NativeMethods.TIFFSeekProc) SeekProc);
			GC.KeepAlive((NativeMethods.TIFFCloseProc) CloseProc);
			GC.KeepAlive((NativeMethods.TIFFSizeProc) SizeProc);
			GC.KeepAlive(stream);
			return frames;
		} finally {
			NativeMethods.TIFFClose(tiff);
		}

		nint ReadProc(nint _, nint dataPtr, nint dataSize) {
			var span = new Span<byte>((byte*) dataPtr, int.CreateChecked(dataSize));
			return stream.Read(span);
		}

		nint WriteProc(nint _, nint dataPtr, nint dataSize) {
			stream.Flush();
			var span = new Span<byte>((byte*) dataPtr, int.CreateChecked(dataSize));
			stream.Write(span);
			return dataSize;
		}

		ulong SeekProc(nint _, ulong offset, int whence) {
			var off = long.CreateChecked(offset);
			if (whence == 2) {
				off = -off;
			}

			return (ulong) stream.Seek(off, (SeekOrigin) whence);
		}

		int CloseProc(nint _) {
			stream.Flush();
			stream.Close();
			return 0;
		}

		ulong SizeProc(nint _) => (ulong) stream.Length;
	}

	internal enum TIFFTag : uint {
		ImageWidth = 256,
		ImageLength = 257,
		RowsPerStrip = 278,
		SamplesPerPixel = 277,
		BitsPerSample = 258,
		SampleFormat = 339,
		Orientation = 274,
		PlanarConfig = 284,
		Photometric = 262,
		Compression = 259,
		ExtraSamples = 338,
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	internal enum TIFFPhotometric {
		MinIsWhite = 0,
		MinIsBlack = 1,
		RGB = 2,
		Palette = 3,
		Mask = 4,
		Separated = 5,
		YCbCr = 6,
		CIELab = 8,
		ICCLab = 9,
		ITULab = 10,
		CFA = 32803,
		LogL = 32844,
		LogLuv = 32845,
	}

	internal enum TIFFPlanarConfig {
		Contig = 1,
		Separate = 2,
	}

	internal enum TIFFOrientation {
		TopLeft = 1,
		TopRight = 2,
		BotRight = 3,
		BotLeft = 4,
		LeftTop = 5,
		RightTop = 6,
		RightBot = 7,
		LeftBot = 8,
	}

	internal enum TIFFSampleFormat {
		UInt = 1,
		Int = 2,
		Float = 3,
		Void = 4,
		ComplexInt = 5,
		ComplexFloat = 6,
	}

	internal enum TIFFExtraSamples {
		Unspecified = 0,
		AssociatedAlpha = 1,
		UnassociatedAlpha = 2,
	}

	private static partial class NativeMethods {
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int TIFFCloseProc(nint userdata);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void TIFFMapFileProc(nint userdata, ref nint @base, ref ulong size);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate nint TIFFReadWriteProc(nint userdata, nint data, nint size);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate ulong TIFFSeekProc(nint userdata, ulong offset, int whence);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate ulong TIFFSizeProc(nint userdata);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void TIFFUnmapFileProc(nint userdata, nint @base, ulong size);

		internal const string LibraryName = "tiff";
		internal const DllImportSearchPath SearchPath = DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory | DllImportSearchPath.ApplicationDirectory;

		[LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8), DefaultDllImportSearchPaths(SearchPath)]
		public static partial nint TIFFClientOpen(string name, string mode, nint handle,
			nint readProc,
			nint writeProc,
			nint seekProc,
			nint closeProc,
			nint sizeProc,
			nint mapProc,
			nint unmapProc);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void TIFFSetField(nint tiff, TIFFTag tag, int value);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void TiffGetField(nint tiff, TIFFTag tag, out int value);

		[LibraryImport(LibraryName, EntryPoint = "TIFFSetField"), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void TIFFSetFieldArray(nint tiff, TIFFTag tag, int count, nint array);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void TIFFWriteEncodedStrip(nint tiff, int strip, nint data, uint dataLength);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void TIFFReadEncodedStrip(nint tiff, int strip, nint data, uint dataLength);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void TIFFWriteDirectory(nint tiff);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		[return: MarshalAs(UnmanagedType.I4)]
		public static partial bool TIFFReadDirectory(nint tiff);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial int TIFFNumberOfDirectories(nint tiff);

		[LibraryImport(LibraryName), DefaultDllImportSearchPaths(SearchPath)]
		public static partial void TIFFClose(nint tiff);
	}
}
