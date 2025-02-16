// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Triton.Pixel.Channels;

namespace Triton.Pixel;

public record struct Color<TChannelValue, TChannel> : IColor<Color<TChannelValue, TChannel>, TChannelValue>
	where TChannelValue : unmanaged, INumberBase<TChannelValue>, IMinMaxValue<TChannelValue>
	where TChannel : IChannel {
	private readonly TChannelValue value;

	public Color(TChannelValue value) => this.value = value;

	public TChannelValue R {
		readonly get {
			if (TChannel.IsRed) {
				return TChannel.GetRed(value);
			}

			return NumericConversion.GetMinimumValueSafe<TChannelValue>();
		}
		set => Channel.SetIfRed<TChannel, TChannelValue>(ref value, value);
	}

	public TChannelValue G {
		readonly get {
			if (TChannel.IsGreen) {
				return TChannel.GetGreen(value);
			}

			return NumericConversion.GetMinimumValueSafe<TChannelValue>();
		}
		set => Channel.SetIfGreen<TChannel, TChannelValue>(ref value, value);
	}

	public TChannelValue B {
		readonly get {
			if (TChannel.IsBlue) {
				return TChannel.GetBlue(value);
			}

			return NumericConversion.GetMinimumValueSafe<TChannelValue>();
		}
		set => Channel.SetIfBlue<TChannel, TChannelValue>(ref value, value);
	}

	public TChannelValue A {
		readonly get {
			if (TChannel.IsAlpha) {
				return TChannel.GetAlpha(value);
			}

			return NumericConversion.GetMaximumValueSafe<TChannelValue>();
		}
		set => Channel.SetIfAlpha<TChannel, TChannelValue>(ref value, value);
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

	static bool IColor.HasRedChannel => TChannel.IsRed;
	static bool IColor.HasGreenChannel => TChannel.IsGreen;
	static bool IColor.HasBlueChannel => TChannel.IsBlue;
	static bool IColor.HasAlphaChannel => TChannel.IsAlpha;
	static bool IColor.ChannelsAreFullyUtilized => TChannel.FullyUtilized;
	static Type IColor.ChannelType => typeof(TChannelValue);

	public static Color<TChannelValue, TChannel> Black => new(TChannel.GetBlack<TChannelValue>());

	public static Color<TChannelValue, TChannel> White => new(TChannel.GetWhite<TChannelValue>());

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B}, A: {A} }}";
}
