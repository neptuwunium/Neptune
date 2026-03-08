// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2 OR LGPL-3.0-or-later
// You may choose either license when using or modifying this code.

using System.Numerics;

namespace Triton.Pixel.Formats;

public record struct ColorBGRA<T>(T B, T G, T R, T A) : IColor<ColorBGRA<T>, T> where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public readonly void GetChannels(out T r, out T g, out T b, out T a) {
		r = R;
		g = G;
		b = B;
		a = A;
	}

	public void SetChannels(T r, T g, T b, T a) {
		R = r;
		G = g;
		B = b;
		A = a;
	}

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => true;
	static bool IColor.HasBlueChannel => true;
	static bool IColor.HasAlphaChannel => true;
	static bool IColor.ChannelsAreFullyUtilized => true;
	static Type IColor.ChannelType => typeof(T);
	static ChannelLayout IColor.ChannelLayout => ChannelLayout.BlueFirst;

	public static ColorBGRA<T> Black => new(NumericConversion.GetMinimumValueSafe<T>(), NumericConversion.GetMinimumValueSafe<T>(), NumericConversion.GetMinimumValueSafe<T>(), NumericConversion.GetMaximumValueSafe<T>());
	public static ColorBGRA<T> White => new(NumericConversion.GetMaximumValueSafe<T>(), NumericConversion.GetMaximumValueSafe<T>(), NumericConversion.GetMaximumValueSafe<T>(), NumericConversion.GetMaximumValueSafe<T>());
	public static ColorBGRA<T> Transparent => Black with { A = NumericConversion.GetMinimumValueSafe<T>() };

	public override string ToString() => $"{{ B: {B}, G: {G}, R: {R}, A: {A} }}";
}
