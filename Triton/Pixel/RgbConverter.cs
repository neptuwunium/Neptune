// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Triton.Pixel;

public static class RgbConverter {
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
}
