// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Triton.Pixel.Channels;

namespace Triton.Pixel;

public record struct Color<TChannelValue, TChannel1, TChannel2> : IColor<Color<TChannelValue, TChannel1, TChannel2>, TChannelValue>
	where TChannelValue : unmanaged, INumberBase<TChannelValue>, IMinMaxValue<TChannelValue>
	where TChannel1 : IChannel
	where TChannel2 : IChannel {
	private TChannelValue Value1;
	private TChannelValue Value2;

	public Color(TChannelValue value1, TChannelValue value2) {
		Value1 = value1;
		Value2 = value2;
	}

	public TChannelValue R {
		readonly get {
			if (TChannel1.IsRed) {
				return TChannel1.GetRed(Value1);
			}

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (TChannel2.IsRed) {
				return TChannel2.GetRed(Value2);
			}

			return NumericConversion.GetMinimumValueSafe<TChannelValue>();
		}
		set {
			Channel.SetIfRed<TChannel1, TChannelValue>(ref Value1, value);
			Channel.SetIfRed<TChannel2, TChannelValue>(ref Value2, value);
		}
	}

	public TChannelValue G {
		readonly get {
			if (TChannel1.IsGreen) {
				return TChannel1.GetGreen(Value1);
			}

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (TChannel2.IsGreen) {
				return TChannel2.GetGreen(Value2);
			}

			return NumericConversion.GetMinimumValueSafe<TChannelValue>();
		}
		set {
			Channel.SetIfGreen<TChannel1, TChannelValue>(ref Value1, value);
			Channel.SetIfGreen<TChannel2, TChannelValue>(ref Value2, value);
		}
	}

	public TChannelValue B {
		readonly get {
			if (TChannel1.IsBlue) {
				return TChannel1.GetBlue(Value1);
			}

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (TChannel2.IsBlue) {
				return TChannel2.GetBlue(Value2);
			}

			return NumericConversion.GetMinimumValueSafe<TChannelValue>();
		}
		set {
			Channel.SetIfBlue<TChannel1, TChannelValue>(ref Value1, value);
			Channel.SetIfBlue<TChannel2, TChannelValue>(ref Value2, value);
		}
	}

	public TChannelValue A {
		readonly get {
			if (TChannel1.IsAlpha) {
				return TChannel1.GetAlpha(Value1);
			}

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (TChannel2.IsAlpha) {
				return TChannel2.GetAlpha(Value2);
			}

			return NumericConversion.GetMaximumValueSafe<TChannelValue>();
		}
		set {
			Channel.SetIfAlpha<TChannel1, TChannelValue>(ref Value1, value);
			Channel.SetIfAlpha<TChannel2, TChannelValue>(ref Value2, value);
		}
	}

	public readonly void GetChannels(out TChannelValue r, out TChannelValue g, out TChannelValue b, out TChannelValue a) {
		r = R;
		g = G;
		b = B;
		a = A;
	}

	public void SetChannels(TChannelValue r, TChannelValue g, TChannelValue b, TChannelValue a) {
		R = r;
		G = g;
		B = b;
		A = a;
	}

	static bool IColor.HasRedChannel => TChannel1.IsRed || TChannel2.IsRed;
	static bool IColor.HasGreenChannel => TChannel1.IsGreen || TChannel2.IsGreen;
	static bool IColor.HasBlueChannel => TChannel1.IsBlue || TChannel2.IsBlue;
	static bool IColor.HasAlphaChannel => TChannel1.IsAlpha || TChannel2.IsAlpha;
	static bool IColor.ChannelsAreFullyUtilized => TChannel1.FullyUtilized && TChannel2.FullyUtilized;
	static Type IColor.ChannelType => typeof(TChannelValue);

	static ChannelLayout IColor.ChannelLayout => TChannel1.IsRed ? ChannelLayout.RedFirst :
		TChannel1.IsGreen ? ChannelLayout.GreenFirst :
		TChannel1.IsBlue ? ChannelLayout.BlueFirst :
		TChannel2.IsRed ? ChannelLayout.AlphaRedFirst :
		TChannel2.IsGreen ? ChannelLayout.AlphaGreenFirst : ChannelLayout.AlphaBlueFirst;

	public static Color<TChannelValue, TChannel1, TChannel2> Black => new(TChannel1.GetBlack<TChannelValue>(), TChannel2.GetBlack<TChannelValue>());

	public static Color<TChannelValue, TChannel1, TChannel2> White => new(TChannel1.GetWhite<TChannelValue>(), TChannel2.GetWhite<TChannelValue>());

	public static Color<TChannelValue, TChannel1, TChannel2> Transparent => Black;

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B}, A: {A} }}";
}
