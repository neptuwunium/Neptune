// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Triton.Pixel.Formats;

public record struct ColorRGB<T> : IColor<ColorRGB<T>, T> where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public ColorRGB(T r, T g, T b) {
		R = r;
		G = g;
		B = b;
	}

	public T R { get; set; }

	public T G { get; set; }

	public T B { get; set; }

	public readonly T A {
		get => NumericConversion.GetMaximumValueSafe<T>();
		set { }
	}

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
	}

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => true;
	static bool IColor.HasBlueChannel => true;
	static bool IColor.HasAlphaChannel => false;
	static bool IColor.ChannelsAreFullyUtilized => true;
	static Type IColor.ChannelType => typeof(T);

	public static ColorRGB<T> Black => new(NumericConversion.GetMinimumValueSafe<T>(), NumericConversion.GetMinimumValueSafe<T>(), NumericConversion.GetMinimumValueSafe<T>());
	public static ColorRGB<T> White => new(NumericConversion.GetMaximumValueSafe<T>(), NumericConversion.GetMaximumValueSafe<T>(), NumericConversion.GetMaximumValueSafe<T>());
	public static ColorRGB<T> Transparent => Black;

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B} }}";
}
