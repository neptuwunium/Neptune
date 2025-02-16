// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Triton.Pixel;

public static class RgbConverter {
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int A8ToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return A8ToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int A8ToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				output[oo + 0] = 0; // b
				output[oo + 1] = 0; // g
				output[oo + 2] = 0; // r
				output[oo + 3] = input[io]; // a
				oo += 4;
				io++;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGB24ToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RGB24ToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGB24ToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				output[oo + 0] = input[io + 2]; // b
				output[oo + 1] = input[io + 1]; // g
				output[oo + 2] = input[io + 0]; // r
				output[oo + 3] = 255; // a
				io += 3;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGBA32ToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RGBA32ToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGBA32ToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				output[oo + 0] = input[io + 2]; // b
				output[oo + 1] = input[io + 1]; // g
				output[oo + 2] = input[io + 0]; // r
				output[oo + 3] = input[io + 3]; // a
				io += 4;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int ARGB32ToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return ARGB32ToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int ARGB32ToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				output[oo + 0] = input[io + 3]; // b
				output[oo + 1] = input[io + 2]; // g
				output[oo + 2] = input[io + 1]; // r
				output[oo + 3] = input[io + 0]; // a
				io += 4;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGB16ToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RGB16ToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGB16ToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				var r = (byte) (input[io + 1] & 0xF8);
				var g = unchecked((byte) ((input[io + 1] << 5) | ((input[io + 0] & 0xE0) >> 3)));
				var b = unchecked((byte) (input[io + 0] << 3));
				output[oo + 0] = b; // b
				output[oo + 1] = g; // g
				output[oo + 2] = r; // r
				output[oo + 3] = 255; // a
				io += 2;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RG16ToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RG16ToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RG16ToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				output[oo + 0] = 0; // b
				output[oo + 1] = input[io + 1]; // g
				output[oo + 2] = input[io + 0]; // r
				output[oo + 3] = 255; // a
				io += 2;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int R8ToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return R8ToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int R8ToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				output[oo + 0] = 0; // b
				output[oo + 1] = 0; // g
				output[oo + 2] = input[io + 0]; // r
				output[oo + 3] = 255; // a
				io += 1;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RHalfToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RHalfToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RHalfToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				var r = ClampByte(ToHalf(input, io) * 255f);
				output[oo + 0] = 0; // b
				output[oo + 1] = 0; // g
				output[oo + 2] = r; // r
				output[oo + 3] = 255; // a
				io += 2;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGHalfToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RGHalfToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGHalfToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				var r = ClampByte(ToHalf(input, io + 0) * 255f);
				var g = ClampByte(ToHalf(input, io + 2) * 255f);
				output[oo + 0] = 0; // b
				output[oo + 1] = g; // g
				output[oo + 2] = r; // r
				output[oo + 3] = 255; // a
				io += 4;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGBAHalfToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RGBAHalfToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGBAHalfToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				var r = ClampByte(ToHalf(input, io + 0) * 255f);
				var g = ClampByte(ToHalf(input, io + 2) * 255f);
				var b = ClampByte(ToHalf(input, io + 4) * 255f);
				var a = ClampByte(ToHalf(input, io + 6) * 255f);
				output[oo + 0] = b; // b
				output[oo + 1] = g; // g
				output[oo + 2] = r; // r
				output[oo + 3] = a; // a
				io += 8;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RFloatToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RFloatToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RFloatToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				var r = ClampByte(ToSingle(input, io) * 255f);
				output[oo + 0] = 0; // b
				output[oo + 1] = 0; // g
				output[oo + 2] = r; // r
				output[oo + 3] = 255; // a
				io += 4;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGFloatToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RGFloatToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGFloatToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				var r = ClampByte(ToSingle(input, io + 0) * 255f);
				var g = ClampByte(ToSingle(input, io + 4) * 255f);
				output[oo + 0] = 0; // b
				output[oo + 1] = g; // g
				output[oo + 2] = r; // r
				output[oo + 3] = 255; // a
				io += 8;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGBAFloatToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RGBAFloatToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGBAFloatToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				var r = ClampByte(ToSingle(input, io + 0) * 255f);
				var g = ClampByte(ToSingle(input, io + 4) * 255f);
				var b = ClampByte(ToSingle(input, io + 8) * 255f);
				var a = ClampByte(ToSingle(input, io + 12) * 255f);
				output[oo + 0] = b; // b
				output[oo + 1] = g; // g
				output[oo + 2] = r; // r
				output[oo + 3] = a; // a
				io += 16;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGB9e5FloatToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RGB9e5FloatToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGB9e5FloatToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				var value = BinaryPrimitives.ReadUInt32LittleEndian(input.Slice(io, 4));
				var scale = Math.Pow(2, unchecked((int) (value >> 27) - 24));
				var r = ClampByte(((value >> 0) & 0x1FF) * scale * 255.0);
				var g = ClampByte(((value >> 9) & 0x1FF) * scale * 255.0);
				var b = ClampByte(((value >> 18) & 0x1FF) * scale * 255.0);
				output[oo + 0] = b; // b
				output[oo + 1] = g; // g
				output[oo + 2] = r; // r
				output[oo + 3] = 255; // a
				io += 4;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RG32ToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RG32ToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RG32ToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				output[oo + 0] = 0; // b
				output[oo + 1] = input[io + 3]; // g
				output[oo + 2] = input[io + 1]; // r
				output[oo + 3] = 255; // a
				io += 4;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGB48ToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RGB48ToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGB48ToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				output[oo + 0] = input[io + 5]; // b
				output[oo + 1] = input[io + 3]; // g
				output[oo + 2] = input[io + 1]; // r
				output[oo + 3] = 255; // a
				io += 6;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGBA64ToBGRA32(ReadOnlySpan<byte> input, int width, int height, out byte[] output) {
		output = new byte[width * height * 4];
		return RGBA64ToBGRA32(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int RGBA64ToBGRA32(ReadOnlySpan<byte> input, int width, int height, Span<byte> output) {
		var io = 0;
		var oo = 0;
		for (var i = 0; i < width; i++) {
			for (var j = 0; j < height; j++) {
				output[oo + 0] = input[io + 5]; // b
				output[oo + 1] = input[io + 3]; // g
				output[oo + 2] = input[io + 1]; // r
				output[oo + 3] = input[io + 7]; // a
				io += 8;
				oo += 4;
			}
		}

		return io;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int Convert<TSourceColor, TSourceChannel, TDestinationColor, TDestinationChannel>(ReadOnlySpan<byte> input, int width, int height, out byte[] output)
		where TSourceChannel : unmanaged
		where TSourceColor : unmanaged, IColor<TSourceChannel>
		where TDestinationChannel : unmanaged
		where TDestinationColor : unmanaged, IColor<TDestinationChannel> {
		output = new byte[width * height * Unsafe.SizeOf<TDestinationColor>()];
		return Convert<TSourceColor, TSourceChannel, TDestinationColor, TDestinationChannel>(input, width, height, output);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static int Convert<TSourceColor, TSourceChannel, TDestinationColor, TDestinationChannel>(ReadOnlySpan<byte> input, int width, int height, Span<byte> output)
		where TSourceChannel : unmanaged
		where TSourceColor : unmanaged, IColor<TSourceChannel>
		where TDestinationChannel : unmanaged
		where TDestinationColor : unmanaged, IColor<TDestinationChannel> {
		var sourceSpan = MemoryMarshal.Cast<byte, TSourceColor>(input)[..(width * height)];
		var destinationSpan = MemoryMarshal.Cast<byte, TDestinationColor>(output)[..(width * height)];
		Convert<TSourceColor, TSourceChannel, TDestinationColor, TDestinationChannel>(sourceSpan, destinationSpan);
		return width * height * Unsafe.SizeOf<TSourceColor>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static void Convert<TSourceColor, TSourceChannel, TDestinationColor, TDestinationChannel>(ReadOnlySpan<TSourceColor> sourceSpan, Span<TDestinationColor> destinationSpan)
		where TSourceChannel : unmanaged
		where TSourceColor : unmanaged, IColor<TSourceChannel>
		where TDestinationChannel : unmanaged
		where TDestinationColor : unmanaged, IColor<TDestinationChannel> {
		for (var i = 0; i < sourceSpan.Length; i++) {
			destinationSpan[i] = sourceSpan[i].Convert<TSourceColor, TSourceChannel, TDestinationColor, TDestinationChannel>();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static float ToHalf(ReadOnlySpan<byte> input, int offset) => (float) BinaryPrimitives.ReadHalfLittleEndian(input.Slice(offset, 2));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static float ToSingle(ReadOnlySpan<byte> input, int offset) => BinaryPrimitives.ReadSingleLittleEndian(input.Slice(offset, 4));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static byte ClampByte(float x) => byte.MaxValue < x ? byte.MaxValue : x > byte.MinValue ? (byte) x : byte.MinValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static byte ClampByte(double x) => byte.MaxValue < x ? byte.MaxValue : x > byte.MinValue ? (byte) x : byte.MinValue;
}
