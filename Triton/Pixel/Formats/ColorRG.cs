// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Triton.Pixel.Formats;

public record struct ColorRG<T> : IColor<ColorRG<T>, T> where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public ColorRG(T r, T g) {
		R = r;
		G = g;
	}

	public T R { get; set; }

	public T G { get; set; }

	public readonly T B {
		get => NumericConversion.GetMinimumValueSafe<T>();
		set { }
	}

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
	}

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => true;
	static bool IColor.HasBlueChannel => false;
	static bool IColor.HasAlphaChannel => false;
	static bool IColor.ChannelsAreFullyUtilized => true;
	static Type IColor.ChannelType => typeof(T);

	public static ColorRG<T> Black => new(NumericConversion.GetMinimumValueSafe<T>(), NumericConversion.GetMinimumValueSafe<T>());
	public static ColorRG<T> White => new(NumericConversion.GetMaximumValueSafe<T>(), NumericConversion.GetMaximumValueSafe<T>());

	public override string ToString() => $"{{ R: {R}, G: {G} }}";
}
