// SPDX-FileCopyrightText: 2022 - 2026 ds5678
// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Triton.Pixel;

public static class Color {
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static void SetConvertedChannels<TThis, TThisChannel, TSourceChannel>(this ref TThis color, TSourceChannel r, TSourceChannel g, TSourceChannel b, TSourceChannel a)
		where TThisChannel : unmanaged, INumberBase<TThisChannel>
		where TSourceChannel : unmanaged
		where TThis : unmanaged, IColor<TThis, TThisChannel> =>
		color.SetChannels(
			NumericConversion.Convert<TSourceChannel, TThisChannel>(r),
			NumericConversion.Convert<TSourceChannel, TThisChannel>(g),
			NumericConversion.Convert<TSourceChannel, TThisChannel>(b),
			NumericConversion.Convert<TSourceChannel, TThisChannel>(a));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static TTarget Convert<TThis, TThisChannelValue, TTarget, TTargetChannelValue>(this TThis color)
		where TThisChannelValue : unmanaged, INumberBase<TThisChannelValue>
		where TTargetChannelValue : unmanaged, INumberBase<TTargetChannelValue>
		where TThis : unmanaged, IColor<TThis, TThisChannelValue>
		where TTarget : unmanaged, IColor<TTarget, TTargetChannelValue> {
		if (typeof(TThis) == typeof(TTarget)) {
			return Unsafe.As<TThis, TTarget>(ref color);
		}

		TTarget destination = default;
		color.GetChannels(out var r, out var g, out var b, out var a);
		if (!TThis.ChannelsAreFullyUtilized && r is not Half or float or double) {
			TTarget.White.GetChannels(out var rW, out var gW, out var bW, out var aW);
			var rF = float.CreateChecked(r) / float.CreateChecked(rW);
			var gF = float.CreateChecked(g) / float.CreateChecked(gW);
			var bF = float.CreateChecked(b) / float.CreateChecked(bW);
			var aF = float.CreateChecked(a) / float.CreateChecked(aW);
			destination.SetConvertedChannels<TTarget, TTargetChannelValue, float>(rF, gF, bF, aF);
		} else {
			destination.SetConvertedChannels<TTarget, TTargetChannelValue, TThisChannelValue>(r, g, b, a);
		}

		return destination;
	}
}
