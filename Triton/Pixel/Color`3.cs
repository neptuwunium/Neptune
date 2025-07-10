// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Triton.Pixel.Channels;

namespace Triton.Pixel;

public record struct Color<TChannelValue, TChannel1, TChannel2, TChannel3> : IColor<Color<TChannelValue, TChannel1, TChannel2, TChannel3>, TChannelValue>
	where TChannelValue : unmanaged, INumberBase<TChannelValue>, IMinMaxValue<TChannelValue>
	where TChannel1 : IChannel
	where TChannel2 : IChannel
	where TChannel3 : IChannel {
	private TChannelValue Value1;
	private TChannelValue Value2;
	private TChannelValue Value3;

	public Color(TChannelValue value1, TChannelValue value2, TChannelValue value3) {
		Value1 = value1;
		Value2 = value2;
		Value3 = value3;
	}

	public TChannelValue R {
		readonly get {
			if (TChannel1.IsRed) {
				return TChannel1.GetRed(Value1);
			}

			if (TChannel2.IsRed) {
				return TChannel2.GetRed(Value2);
			}

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (TChannel3.IsRed) {
				return TChannel3.GetRed(Value3);
			}

			return NumericConversion.GetMinimumValueSafe<TChannelValue>();
		}
		set {
			Channel.SetIfRed<TChannel1, TChannelValue>(ref Value1, value);
			Channel.SetIfRed<TChannel2, TChannelValue>(ref Value2, value);
			Channel.SetIfRed<TChannel3, TChannelValue>(ref Value3, value);
		}
	}

	public TChannelValue G {
		readonly get {
			if (TChannel1.IsGreen) {
				return TChannel1.GetGreen(Value1);
			}

			if (TChannel2.IsGreen) {
				return TChannel2.GetGreen(Value2);
			}

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (TChannel3.IsGreen) {
				return TChannel3.GetGreen(Value3);
			}

			return NumericConversion.GetMinimumValueSafe<TChannelValue>();
		}
		set {
			Channel.SetIfGreen<TChannel1, TChannelValue>(ref Value1, value);
			Channel.SetIfGreen<TChannel2, TChannelValue>(ref Value2, value);
			Channel.SetIfGreen<TChannel3, TChannelValue>(ref Value3, value);
		}
	}

	public TChannelValue B {
		readonly get {
			if (TChannel1.IsBlue) {
				return TChannel1.GetBlue(Value1);
			}

			if (TChannel2.IsBlue) {
				return TChannel2.GetBlue(Value2);
			}

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (TChannel3.IsBlue) {
				return TChannel3.GetBlue(Value3);
			}

			return NumericConversion.GetMinimumValueSafe<TChannelValue>();
		}
		set {
			Channel.SetIfBlue<TChannel1, TChannelValue>(ref Value1, value);
			Channel.SetIfBlue<TChannel2, TChannelValue>(ref Value2, value);
			Channel.SetIfBlue<TChannel3, TChannelValue>(ref Value3, value);
		}
	}

	public TChannelValue A {
		readonly get {
			if (TChannel1.IsAlpha) {
				return TChannel1.GetAlpha(Value1);
			}

			if (TChannel2.IsAlpha) {
				return TChannel2.GetAlpha(Value2);
			}

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (TChannel3.IsAlpha) {
				return TChannel3.GetAlpha(Value3);
			}

			return NumericConversion.GetMaximumValueSafe<TChannelValue>();
		}
		set {
			Channel.SetIfAlpha<TChannel1, TChannelValue>(ref Value1, value);
			Channel.SetIfAlpha<TChannel2, TChannelValue>(ref Value2, value);
			Channel.SetIfAlpha<TChannel3, TChannelValue>(ref Value3, value);
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

	static bool IColor.HasRedChannel => TChannel1.IsRed || TChannel2.IsRed || TChannel3.IsRed;
	static bool IColor.HasGreenChannel => TChannel1.IsGreen || TChannel2.IsGreen || TChannel3.IsGreen;
	static bool IColor.HasBlueChannel => TChannel1.IsBlue || TChannel2.IsBlue || TChannel3.IsBlue;
	static bool IColor.HasAlphaChannel => TChannel1.IsAlpha || TChannel2.IsAlpha || TChannel3.IsAlpha;
	static bool IColor.ChannelsAreFullyUtilized => TChannel1.FullyUtilized && TChannel2.FullyUtilized && TChannel3.FullyUtilized;
	static Type IColor.ChannelType => typeof(TChannelValue);

	static ChannelLayout IColor.ChannelLayout => TChannel1.IsRed ? ChannelLayout.RedFirst :
		TChannel1.IsGreen ? ChannelLayout.GreenFirst :
		TChannel1.IsBlue ? ChannelLayout.BlueFirst :
		TChannel2.IsRed ? ChannelLayout.AlphaRedFirst :
		TChannel2.IsGreen ? ChannelLayout.AlphaGreenFirst : ChannelLayout.AlphaBlueFirst;

	public static Color<TChannelValue, TChannel1, TChannel2, TChannel3> Black => new(TChannel1.GetBlack<TChannelValue>(), TChannel2.GetBlack<TChannelValue>(), TChannel3.GetBlack<TChannelValue>());

	public static Color<TChannelValue, TChannel1, TChannel2, TChannel3> White => new(TChannel1.GetWhite<TChannelValue>(), TChannel2.GetWhite<TChannelValue>(), TChannel3.GetWhite<TChannelValue>());

	public static Color<TChannelValue, TChannel1, TChannel2, TChannel3> Transparent => Black;

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B}, A: {A} }}";
}
