// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Formats;

public record struct ColorR5G6B5 : IColor<ColorR5G6B5, float> {
	public ushort Value { get; set; }

	public float R {
		readonly get => (Value & 0x1F) / 31.0f;
		set => Value = (ushort) ((Value & 0xFFE0) | (ushort) (value * 0x1F));
	}

	public float G {
		readonly get => ((Value >> 5) & 0x3F) / 63.0f;
		set => Value = (ushort) ((Value & 0xF81F) | ((ushort) (value * 0x3F) << 5));
	}

	public float B {
		readonly get => ((Value >> 1) & 0x1F) / 31.0f;
		set => Value = (ushort) ((Value & 0x07FF) | ((ushort) (value * 0x1F) << 11));
	}

	public float A {
		readonly get => 1.0f;
		set { }
	}

	public readonly void GetChannels(out float r, out float g, out float b, out float a) {
		r = R;
		g = G;
		b = B;
		a = A;
	}

	public void SetChannels(float r, float g, float b, float a) {
		R = r;
		G = g;
		B = b;
	}

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => true;
	static bool IColor.HasBlueChannel => true;
	static bool IColor.HasAlphaChannel => false;
	static bool IColor.ChannelsAreFullyUtilized => false;
	static Type IColor.ChannelType => typeof(float);
	static ChannelLayout IColor.ChannelLayout => ChannelLayout.RedFirst;

	public static ColorR5G6B5 Black => new();

	public static ColorR5G6B5 White => new() { R = 1, G = 1, B = 1 };
	public static ColorR5G6B5 Transparent => Black;

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B} }}";
}
