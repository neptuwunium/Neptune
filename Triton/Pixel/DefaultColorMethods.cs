// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Triton.Pixel;

internal static class DefaultColorMethods {
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static void GetChannels<T, TArg>(T color, out TArg r, out TArg g, out TArg b, out TArg a)
		where TArg : unmanaged, INumberBase<TArg>
		where T : unmanaged, IColor<T, TArg> {
		r = color.R;
		g = color.G;
		b = color.B;
		a = color.A;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static void SetChannels<T, TArg>(ref T color, TArg r, TArg g, TArg b, TArg a)
		where TArg : unmanaged, INumberBase<TArg>
		where T : unmanaged, IColor<T, TArg> {
		color.R = r;
		color.G = g;
		color.B = b;
		color.A = a;
	}
}
