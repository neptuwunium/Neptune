// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Formats;

public record struct ColorB5G5R5A1 : IColor<ColorB5G5R5A1, float> {
	public ushort Value { get; set; }

	public float R {
		readonly get => (Value & 0x1F) / 31.0f;
		set => Value = (ushort) ((Value & 0xFFE0) | (ushort) (value * 0x1F));
	}

	public float G {
		readonly get => ((Value >> 5) & 0x1F) / 31.0f;
		set => Value = (ushort) ((Value & 0xFC1F) | ((ushort) (value * 0x1F) << 5));
	}

	public float B {
		readonly get => ((Value >> 10) & 0x1F) / 31.0f;
		set => Value = (ushort) ((Value & 0x83FF) | ((ushort) (value * 0x1F) << 10));
	}

	public float A {
		readonly get => (Value >> 15) & 1;
		set => Value = (ushort) ((Value & 0x7FFF) | ((ushort) value << 15));
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
		A = a;
	}

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => true;
	static bool IColor.HasBlueChannel => true;
	static bool IColor.HasAlphaChannel => true;
	static bool IColor.ChannelsAreFullyUtilized => false;
	static Type IColor.ChannelType => typeof(float);
	static ChannelLayout IColor.ChannelLayout => ChannelLayout.BlueFirst;

	public static ColorB5G5R5A1 Black => new() { A = 1 };

	public static ColorB5G5R5A1 White => new() { R = 1, G = 1, B = 1, A = 1 };
	public static ColorB5G5R5A1 Transparent => Black with { A = 0 };

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B} }}";
}
