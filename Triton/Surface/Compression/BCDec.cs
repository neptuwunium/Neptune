// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Triton.Surface.Compression;

public static partial class BCDec {
	public const int BC1BlockSize = 8;
	public const int BC2BlockSize = 16;
	public const int BC3BlockSize = 16;
	public const int BC4BlockSize = 8;
	public const int BC5BlockSize = 16;
	public const int BC6HBlockSize = 16;
	public const int BC7BlockSize = 16;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int CalculateBC1Size(int width, int height) => (width >> 2) * (height >> 2) * BC1BlockSize;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int CalculateBC2Size(int width, int height) => (width >> 2) * (height >> 2) * BC2BlockSize;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int CalculateBC3Size(int width, int height) => (width >> 2) * (height >> 2) * BC3BlockSize;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int CalculateBC4Size(int width, int height) => (width >> 2) * (height >> 2) * BC4BlockSize;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int CalculateBC5Size(int width, int height) => (width >> 2) * (height >> 2) * BC5BlockSize;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int CalculateBC6HSize(int width, int height) => (width >> 2) * (height >> 2) * BC6HBlockSize;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int CalculateBC7Size(int width, int height) => (width >> 2) * (height >> 2) * BC7BlockSize;

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public static void DecompressBC1(ReadOnlyMemory<byte> compressed, Memory<byte> decompressed, int width, int height) {
		if (compressed.Length < CalculateBC1Size(width, height)) {
			throw new IndexOutOfRangeException("Compressed is too small");
		}

		if (decompressed.Length < width * height * 4) {
			throw new IndexOutOfRangeException("Decompressed is too small");
		}

		using var srcPin = compressed.Pin();
		using var dstPin = decompressed.Pin();
		unsafe {
			var src = (nint) srcPin.Pointer;

			for (var heightIndex = 0; heightIndex < height; heightIndex += 4) {
				for (var widthIndex = 0; widthIndex < width; widthIndex += 4) {
					var dst = (nint) dstPin.Pointer + (heightIndex * width + widthIndex) * 4;
					NativeMethods.bcdec_bc1(src, dst, width * 4);
					src += BC1BlockSize;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public static void DecompressBC2(ReadOnlyMemory<byte> compressed, Memory<byte> decompressed, int width, int height) {
		if (compressed.Length < CalculateBC2Size(width, height)) {
			throw new IndexOutOfRangeException("Compressed is too small");
		}

		if (decompressed.Length < width * height * 4) {
			throw new IndexOutOfRangeException("Decompressed is too small");
		}

		using var srcPin = compressed.Pin();
		using var dstPin = decompressed.Pin();
		unsafe {
			var src = (nint) srcPin.Pointer;

			for (var heightIndex = 0; heightIndex < height; heightIndex += 4) {
				for (var widthIndex = 0; widthIndex < width; widthIndex += 4) {
					var dst = (nint) dstPin.Pointer + (heightIndex * width + widthIndex) * 4;
					NativeMethods.bcdec_bc2(src, dst, width * 4);
					src += BC2BlockSize;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public static void DecompressBC3(ReadOnlyMemory<byte> compressed, Memory<byte> decompressed, int width, int height) {
		if (compressed.Length < CalculateBC3Size(width, height)) {
			throw new IndexOutOfRangeException("Compressed is too small");
		}

		if (decompressed.Length < width * height * 4) {
			throw new IndexOutOfRangeException("Decompressed is too small");
		}

		using var srcPin = compressed.Pin();
		using var dstPin = decompressed.Pin();
		unsafe {
			var src = (nint) srcPin.Pointer;

			for (var heightIndex = 0; heightIndex < height; heightIndex += 4) {
				for (var widthIndex = 0; widthIndex < width; widthIndex += 4) {
					var dst = (nint) dstPin.Pointer + (heightIndex * width + widthIndex) * 4;
					NativeMethods.bcdec_bc3(src, dst, width * 4);
					src += BC3BlockSize;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public static void DecompressBC4(ReadOnlyMemory<byte> compressed, Memory<byte> decompressed, int width, int height, bool isSigned) {
		if (compressed.Length < CalculateBC4Size(width, height)) {
			throw new IndexOutOfRangeException("Compressed is too small");
		}

		if (decompressed.Length < width * height) {
			throw new IndexOutOfRangeException("Decompressed is too small");
		}

		using var srcPin = compressed.Pin();
		using var dstPin = decompressed.Pin();
		unsafe {
			var src = (nint) srcPin.Pointer;

			for (var heightIndex = 0; heightIndex < height; heightIndex += 4) {
				for (var widthIndex = 0; widthIndex < width; widthIndex += 4) {
					var dst = (nint) dstPin.Pointer + (heightIndex * width + widthIndex);
					NativeMethods.bcdec_bc4(src, dst, width, isSigned);
					src += BC4BlockSize;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public static void DecompressBC5(ReadOnlyMemory<byte> compressed, Memory<byte> decompressed, int width, int height, bool isSigned) {
		if (compressed.Length < CalculateBC5Size(width, height)) {
			throw new IndexOutOfRangeException("Compressed is too small");
		}

		if (decompressed.Length < width * height * 2) {
			throw new IndexOutOfRangeException("Decompressed is too small");
		}

		using var srcPin = compressed.Pin();
		using var dstPin = decompressed.Pin();
		unsafe {
			var src = (nint) srcPin.Pointer;

			for (var heightIndex = 0; heightIndex < height; heightIndex += 4) {
				for (var widthIndex = 0; widthIndex < width; widthIndex += 4) {
					var dst = (nint) dstPin.Pointer + (heightIndex * width + widthIndex) * 2;
					NativeMethods.bcdec_bc5(src, dst, width * 2, isSigned);
					src += BC5BlockSize;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static float ComputeNormalZ(float x, float y) {
		var nx = 2 * x - 1;
		var ny = 2 * y - 1;
		var nz2 = 1 - nx * nx - ny * ny;
		var nz = nz2 > 0 ? Math.Sqrt(nz2) : 0;
		return (float) ((nz + 1) / 2.0d);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static byte ComputeNormalZ(byte x, byte y) => byte.CreateSaturating(ComputeNormalZ(x / 255.0f, y / 255.0f) * 255.0f);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static void ComputeNormal(Span<float> data, int stride = 4, int xIndex = 0, int yIndex = 1, int zIndex = 2) {
		for (var index = 0; index < data.Length; index += stride) {
			data[index + zIndex] = ComputeNormalZ(data[index + xIndex], data[index + yIndex]);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static void ComputeNormal(Span<byte> data, int stride = 4, int xIndex = 0, int yIndex = 1, int zIndex = 2) {
		for (var index = 0; index < data.Length; index += stride) {
			data[index + zIndex] = ComputeNormalZ(data[index + xIndex], data[index + yIndex]);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public static void DecompressBC5Normal(ReadOnlyMemory<byte> compressed, Memory<byte> decompressed, int width, int height, bool isSigned) {
		if (compressed.Length < CalculateBC5Size(width, height)) {
			throw new IndexOutOfRangeException("Compressed is too small");
		}

		if (decompressed.Length < width * height * 3) {
			throw new IndexOutOfRangeException("Decompressed is too small");
		}

		using var srcPin = compressed.Pin();
		unsafe {
			var src = (nint) srcPin.Pointer;

			var tmp = stackalloc byte[32]; // 4 * 4 * 2
			var tmpSpan = new Span<byte>(tmp, 32);
			var dst = decompressed.Span;

			for (var heightIndex = 0; heightIndex < height; heightIndex += 4) {
				for (var widthIndex = 0; widthIndex < width; widthIndex += 4) {
					NativeMethods.bcdec_bc5(src, (nint) tmp, 8, isSigned);
					src += BC5BlockSize;

					var dstBlock = dst[((heightIndex * width + widthIndex) * 3)..];
					var dstIndex = 0;
					var tmpIndex = 0;
					for (var heightTmp = 0; heightTmp < 4; ++heightTmp) {
						for (var widthTmp = 0; widthTmp < 4; ++widthTmp, tmpIndex += 2) {
							var x = dstBlock[dstIndex + widthTmp * 3 + 0] = tmpSpan[tmpIndex];
							var y = dstBlock[dstIndex + widthTmp * 3 + 1] = tmpSpan[tmpIndex + 1];
							dstBlock[dstIndex + widthTmp * 3 + 2] = ComputeNormalZ(x, y);
						}

						dstIndex += width * 3;
					}
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public static void DecompressBC6HFloat(ReadOnlyMemory<byte> compressed, Memory<byte> decompressed, int width, int height, bool isSigned) {
		if (compressed.Length < CalculateBC6HSize(width, height)) {
			throw new IndexOutOfRangeException("Compressed is too small");
		}

		if (decompressed.Length < width * height * 12) {
			throw new IndexOutOfRangeException("Decompressed is too small");
		}

		using var srcPin = compressed.Pin();
		using var dstPin = decompressed.Pin();
		unsafe {
			var src = (nint) srcPin.Pointer;

			for (var heightIndex = 0; heightIndex < height; heightIndex += 4) {
				for (var widthIndex = 0; widthIndex < width; widthIndex += 4) {
					var dst = (nint) dstPin.Pointer + (heightIndex * width + widthIndex) * 12;
					NativeMethods.bcdec_bc6h_float(src, dst, width * 3, isSigned);
					src += BC6HBlockSize;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public static void DecompressBC6H(ReadOnlyMemory<byte> compressed, Memory<byte> decompressed, int width, int height, bool isSigned) {
		if (compressed.Length < CalculateBC6HSize(width, height)) {
			throw new IndexOutOfRangeException("Compressed is too small");
		}

		if (decompressed.Length < width * height * 6) {
			throw new IndexOutOfRangeException("Decompressed is too small");
		}

		using var srcPin = compressed.Pin();
		using var dstPin = decompressed.Pin();
		unsafe {
			var src = (nint) srcPin.Pointer;

			for (var heightIndex = 0; heightIndex < height; heightIndex += 4) {
				for (var widthIndex = 0; widthIndex < width; widthIndex += 4) {
					var dst = (nint) dstPin.Pointer + (heightIndex * width + widthIndex) * 6;
					NativeMethods.bcdec_bc6h_half(src, dst, width * 3, isSigned);
					src += BC6HBlockSize;
				}
			}
		}
	}

	public static void DecompressBC7(ReadOnlyMemory<byte> compressed, Memory<byte> decompressed, int width, int height) {
		if (compressed.Length < CalculateBC7Size(width, height)) {
			throw new IndexOutOfRangeException("Compressed is too small");
		}

		if (decompressed.Length < width * height * 4) {
			throw new IndexOutOfRangeException("Decompressed is too small");
		}

		using var srcPin = compressed.Pin();
		using var dstPin = decompressed.Pin();
		unsafe {
			var src = (nint) srcPin.Pointer;

			for (var heightIndex = 0; heightIndex < height; heightIndex += 4) {
				for (var widthIndex = 0; widthIndex < width; widthIndex += 4) {
					var dst = (nint) dstPin.Pointer + (heightIndex * width + widthIndex) * 4;
					NativeMethods.bcdec_bc7(src, dst, width * 4);
					src += BC7BlockSize;
				}
			}
		}
	}

	internal static partial class NativeMethods {
		private const string LibName = "bcdec";

		[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
		internal static partial nint bcdec_bc1(nint compressedBlock, nint decompressedBlock, int destinationPitch);

		[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
		internal static partial nint bcdec_bc2(nint compressedBlock, nint decompressedBlock, int destinationPitch);

		[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
		internal static partial nint bcdec_bc3(nint compressedBlock, nint decompressedBlock, int destinationPitch);

		[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
		internal static partial nint bcdec_bc4(nint compressedBlock, nint decompressedBlock, int destinationPitch, [MarshalAs(UnmanagedType.I4)] bool isSigned);

		[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
		internal static partial nint bcdec_bc5(nint compressedBlock, nint decompressedBlock, int destinationPitch, [MarshalAs(UnmanagedType.I4)] bool isSigned);

		[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
		internal static partial nint bcdec_bc4_float(nint compressedBlock, nint decompressedBlock, int destinationPitch, [MarshalAs(UnmanagedType.I4)] bool isSigned);

		[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
		internal static partial nint bcdec_bc5_float(nint compressedBlock, nint decompressedBlock, int destinationPitch, [MarshalAs(UnmanagedType.I4)] bool isSigned);

		[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
		internal static partial nint bcdec_bc6h_float(nint compressedBlock, nint decompressedBlock, int destinationPitch, [MarshalAs(UnmanagedType.I4)] bool isSigned);

		[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
		internal static partial nint bcdec_bc6h_half(nint compressedBlock, nint decompressedBlock, int destinationPitch, [MarshalAs(UnmanagedType.I4)] bool isSigned);

		[LibraryImport(LibName), DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory)]
		internal static partial nint bcdec_bc7(nint compressedBlock, nint decompressedBlock, int destinationPitch);
	}
}
