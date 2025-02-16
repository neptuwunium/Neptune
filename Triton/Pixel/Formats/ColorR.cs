// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Triton.Pixel.Formats;

public record struct ColorR<T> : IColor<ColorR<T>, T> where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public T R { get; set; }

	public readonly T G {
		get => NumericConversion.GetMinimumValueSafe<T>();
		set { }
	}

	public readonly T B {
		get => NumericConversion.GetMinimumValueSafe<T>();
		set { }
	}

	public readonly T A {
		get => NumericConversion.GetMaximumValueSafe<T>();
		set { }
	}

	public ColorR(T r) {
		R = r;
	}

	public readonly void GetChannels(out T r, out T g, out T b, out T a) {
		r = R;
		g = G;
		b = B;
		a = A;
	}

	public void SetChannels(T r, T g, T b, T a) {
		R = r;
	}

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => false;
	static bool IColor.HasBlueChannel => false;
	static bool IColor.HasAlphaChannel => false;
	static bool IColor.ChannelsAreFullyUtilized => true;
	static Type IColor.ChannelType => typeof(T);

	public static ColorR<T> Black => new(NumericConversion.GetMinimumValueSafe<T>());
	public static ColorR<T> White => new(NumericConversion.GetMaximumValueSafe<T>());
	public static ColorR<T> Transparent => Black;

	public override string ToString() {
		return $"{{ R: {R} }}";
	}
}
