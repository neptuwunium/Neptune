// SPDX-FileCopyrightText: 2022 - 2026 ds5678
// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Triton.Pixel.Formats;

public record struct ColorRA<T> : IColor<ColorRA<T>, T> where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public ColorRA(T r, T a) {
		R = r;
		A = a;
	}

	public T R { get; set; }

	public readonly T B {
		get => NumericConversion.GetMinimumValueSafe<T>();
		set { }
	}

	public readonly T G {
		get => NumericConversion.GetMinimumValueSafe<T>();
		set { }
	}

	public T A { get; set; }

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
	static bool IColor.HasGreenChannel => false;
	static bool IColor.HasBlueChannel => false;
	static bool IColor.HasAlphaChannel => true;
	static bool IColor.ChannelsAreFullyUtilized => true;
	static Type IColor.ChannelType => typeof(T);
	static ChannelLayout IColor.ChannelLayout => ChannelLayout.RedFirst;

	public static ColorRA<T> Black => new(NumericConversion.GetMinimumValueSafe<T>(), NumericConversion.GetMinimumValueSafe<T>());
	public static ColorRA<T> White => new(NumericConversion.GetMaximumValueSafe<T>(), NumericConversion.GetMaximumValueSafe<T>());
	public static ColorRA<T> Transparent => Black;

	public override string ToString() => $"{{ R: {R}, A: {A} }}";
}
