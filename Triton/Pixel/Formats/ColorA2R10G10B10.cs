// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Formats;

public record struct ColorA2R10G10B10 : IColor<ColorA2R10G10B10, float> {
	public uint Value { get; set; }

	public float R {
		readonly get => ((Value >> 3) & 0x3FF) / 1023.0f;
		set => Value = (Value & 0xFFFFE007U) | ((uint) (value * 0x3FF) << 3);
	}

	public float G {
		readonly get => ((Value >> 13) & 0x3FF) / 1023.0f;
		set => Value = (Value & 0xFF801FFFU) | ((uint) (value * 0x3FF) << 13);
	}

	public float B {
		readonly get => ((Value >> 23) & 0x3FF) / 1023.0f;
		set => Value = (Value & 0x7FFFFFU) | ((uint) (value * 0x3FF) << 23);
	}

	public float A {
		readonly get => (Value & 3) / 3f;
		set => Value = (Value & 0xFFFFFFFCU) | (uint) (value * 3);
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

	public static ColorA2R10G10B10 Black => new() { A = 1 };

	public static ColorA2R10G10B10 White => new() { R = 1, G = 1, B = 1, A = 1 };
	public static ColorA2R10G10B10 Transparent => Black with { A = 0 };

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B}, A: {A} }}";
}
