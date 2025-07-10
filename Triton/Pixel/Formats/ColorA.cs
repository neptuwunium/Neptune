// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Triton.Pixel.Formats;

public record struct ColorA<T> : IColor<ColorA<T>, T> where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public ColorA(T a) => A = a;

	public readonly T R {
		get => NumericConversion.GetMinimumValueSafe<T>();
		set { }
	}

	public readonly T G {
		get => NumericConversion.GetMinimumValueSafe<T>();
		set { }
	}

	public readonly T B {
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

	public void SetChannels(T r, T g, T b, T a) => A = a;

	static bool IColor.HasRedChannel => false;
	static bool IColor.HasGreenChannel => false;
	static bool IColor.HasBlueChannel => false;
	static bool IColor.HasAlphaChannel => true;
	static bool IColor.ChannelsAreFullyUtilized => true;
	static Type IColor.ChannelType => typeof(T);
	static ChannelLayout IColor.ChannelLayout => ChannelLayout.RedFirst;
	public static ColorA<T> Black => new(NumericConversion.GetMaximumValueSafe<T>());
	public static ColorA<T> White => new(NumericConversion.GetMinimumValueSafe<T>());
	public static ColorA<T> Transparent => White;
}
