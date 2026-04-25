// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;

namespace Triton.Pixel.Formats;

public record struct ColorR11G11B10 : IColor<ColorR11G11B10, float> {
	public uint Value { get; set; }

	public float R {
		readonly get => (float) Unsafe.BitCast<ushort, Half>((ushort) ((Value & 0x000007FF) << 4));
		set => Value = (Value & 0xFFFFF800U) | (uint) (Unsafe.BitCast<Half, ushort>((Half) value) >> 4);
	}

	public float G {
		readonly get => (float) Unsafe.BitCast<ushort, Half>((ushort) ((Value & 0x003FF800) >> 7));
		set => Value = (Value & 0xFFC007FFU) | ((uint) (Unsafe.BitCast<Half, ushort>((Half) value) >> 4) << 11);
	}

	public float B {
		readonly get => (float) Unsafe.BitCast<ushort, Half>((ushort) ((Value & 0xFFC00000) >> 17));
		set => Value = (Value & 0x3FFFFFU) | ((uint) (Unsafe.BitCast<Half, ushort>((Half) value) >> 5) << 22);
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
