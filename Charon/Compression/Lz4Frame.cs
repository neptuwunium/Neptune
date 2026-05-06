// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Compression;

public static partial class Lz4Frame {
	public static unsafe int Decompress(Memory<byte> src, Memory<byte> dst) {
		var code = NativeMethods.LZ4F_createDecompressionContext(out var dctx, 100);
		if (NativeMethods.LZ4F_isError(code)) {
			throw new InvalidOperationException($"cannot create LZ4F decompression context: {NativeMethods.LZ4F_getErrorName(code)}");
		}

		try {
			using var srcPin = src.Pin();
			using var dstPin = dst.Pin();
			var srcPtr = (byte*) srcPin.Pointer;
			var dstPtr = (byte*) dstPin.Pointer;
			var srcEndPtr = srcPtr + src.Length;
			var dstEndPtr = dstPtr + dst.Length;

			while (srcPtr < srcEndPtr) {
				var outSize = (nuint) (dstEndPtr - dstPtr);
				var inSize = (nuint) (srcEndPtr - srcPtr);

				var next = NativeMethods.LZ4F_decompress(dctx, dstPtr, ref outSize, srcPtr, ref inSize, nint.Zero);

				if (NativeMethods.LZ4F_isError(next)) {
					throw new InvalidDataException($"cannot decompression: {NativeMethods.LZ4F_getErrorName(code)}");
				}

				srcPtr += inSize;
				dstPtr += outSize;

				if (next == 0) {
					break;
				}
			}

			return dst.Length - (int) (dstEndPtr - dstPtr);
		} finally {
			_ = NativeMethods.LZ4F_freeDecompressionContext(dctx);
		}
	}

	private static partial class NativeMethods {
		[LibraryImport(CompressionHelper.Lz4LibraryName)] [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)] [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static unsafe partial nuint LZ4F_createDecompressionContext(out nint dctx, uint version);

		[LibraryImport(CompressionHelper.Lz4LibraryName)] [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)] [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static unsafe partial nuint LZ4F_freeDecompressionContext(nint dctx);

		[LibraryImport(CompressionHelper.Lz4LibraryName)] [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)] [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])] [return: MarshalAs(UnmanagedType.I4)]
		public static unsafe partial bool LZ4F_isError(nuint code);

		[LibraryImport(CompressionHelper.Lz4LibraryName)] [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)] [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])] [return: MarshalAs(UnmanagedType.LPStr)]
		public static unsafe partial string LZ4F_getErrorName(nuint code);

		[LibraryImport(CompressionHelper.Lz4LibraryName)] [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)] [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
		public static unsafe partial nuint LZ4F_decompress(nint dctx, byte* dstBuffer, ref nuint dstSizePtr, byte* srcBuffer, ref nuint srcSizePtr, nint dOptPtr);
	}
}
