// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Formats;

public record struct ColorRGBA16 : IColor<ColorRGBA16, byte> {
	private UInt4Pair Ba;
	private UInt4Pair Rg;

	public byte R {
		readonly get => Rg.HighValue;
		set => Rg.HighValue = value;
	}

	public byte G {
		readonly get => Rg.LowValue;
		set => Rg.LowValue = value;
	}

	public byte B {
		readonly get => Ba.HighValue;
		set => Ba.HighValue = value;
	}

	public byte A {
		readonly get => Ba.LowValue;
		set => Ba.LowValue = value;
	}

	public readonly void GetChannels(out byte r, out byte g, out byte b, out byte a) => DefaultColorMethods.GetChannels(this, out r, out g, out b, out a);

	public void SetChannels(byte r, byte g, byte b, byte a) => DefaultColorMethods.SetChannels(ref this, r, g, b, a);

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => true;
	static bool IColor.HasBlueChannel => true;
	static bool IColor.HasAlphaChannel => true;
	static bool IColor.ChannelsAreFullyUtilized => false;
	static Type IColor.ChannelType => typeof(byte);
	public static ColorRGBA16 Black => new() { A = 0xF };
	public static ColorRGBA16 White => new() { R = 0xF, G = 0xF, B = 0xF, A = 0xF };
	public static ColorRGBA16 Transparent => Black with { A = 0 };

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B}, A: {A} }}";
}
