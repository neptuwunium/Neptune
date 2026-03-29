// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Formats;

public record struct ColorR11G11B10 : IColor<ColorR11G11B10, float> {
	public uint Value { get; set; }

	public float R {
		readonly get => (Value & 0x7FFU) / 2047.0f;
		set => Value = (Value & 0xFFFFF800U) | (uint) (value * 0x7FF);
	}

	public float G {
		readonly get => ((Value >> 11) & 0x7FF) / 2047.0f;
		set => Value = (Value & 0xFFC007FFU) | ((uint) (value * 0x7FF) << 11);
	}

	public float B {
		readonly get => ((Value >> 22) & 0x3FF) / 1023.0f;
		set => Value = (Value & 0x3FFFFFU) | ((uint) (value * 0x3FF) << 22);
	}

	public readonly float A {
		get => 1.0f;
		set { }
	}

	public readonly void GetChannels(out float r, out float g, out float b, out float a) {
		r = R;
		g = G;
		b = B;
		a = 1.0f;
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

	public static ColorR11G11B10 Black => new();

	public static ColorR11G11B10 White => new() { R = 1, G = 1, B = 1 };
	public static ColorR11G11B10 Transparent => Black;

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B} }}";
}
