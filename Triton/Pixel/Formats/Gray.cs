// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Triton.Pixel.Formats;

public record struct Gray<T>(T Value) : IColor<Gray<T>, T> where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public T R {
		readonly get => Value;
		set => Value = value;
	}
	public T G {
		readonly get => Value;
		set => Value = value;
	}

	public T B {
		readonly get => Value;
		set => Value = value;
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

	public void SetChannels(T r, T g, T b, T a) => Value = r;

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => true;
	static bool IColor.HasBlueChannel => true;
	static bool IColor.HasAlphaChannel => false;
	static bool IColor.ChannelsAreFullyUtilized => true;
	static Type IColor.ChannelType => typeof(T);
	static ChannelLayout IColor.ChannelLayout => ChannelLayout.RedFirst;

	public static Gray<T> Black => new(NumericConversion.GetMinimumValueSafe<T>());
	public static Gray<T> White => new(NumericConversion.GetMaximumValueSafe<T>());
	public static Gray<T> Transparent => Black;

	public override string ToString() => $"{{ Value: {Value} }}";
}
