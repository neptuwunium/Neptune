// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Formats;

public record struct ColorB10G10R10A2 : IColor<ColorB10G10R10A2, float> {
	public uint Value { get; set; }

	public float B {
		readonly get => (Value & 0x3FF) / 1023.0f;
		set => Value = (Value & 0xFFFFFC00U) | (uint) (value * 0x3FF);
	}

	public float G {
		readonly get => ((Value >> 10) & 0x3FF) / 1023.0f;
		set => Value = (Value & 0xFFF003FFU) | ((uint) (value * 0x3FF) << 10);
	}

	public float R {
		readonly get => ((Value >> 20) & 0x3FF) / 1023.0f;
		set => Value = (Value & 0xC00FFFFFU) | ((uint) (value * 0x3FF) << 20);
	}

	public float A {
		readonly get => ((Value >> 30) & 3) / 3f;
		set => Value = (Value & 0x3FFFFFFFU) | ((uint) (value * 3) << 30);
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
	static ChannelLayout IColor.ChannelLayout => ChannelLayout.RedFirst;

	public static ColorB10G10R10A2 Black => new() { A = 1 };

	public static ColorB10G10R10A2 White => new() { R = 1, G = 1, B = 1, A = 1 };
	public static ColorB10G10R10A2 Transparent => Black with { A = 0 };

	public override string ToString() => $"{{ B: {B}, G: {G}, R: {R}, A: {A} }}";
}
