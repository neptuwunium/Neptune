// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Formats;

public record struct ColorB4G4R4A4 : IColor<ColorB4G4R4A4, byte> {
	private UInt4Pair Bg;
	private UInt4Pair Ra;

	public byte R {
		readonly get => Ra.HighValue;
		set => Ra.HighValue = value;
	}

	public byte G {
		readonly get => Ra.LowValue;
		set => Ra.LowValue = value;
	}

	public byte B {
		readonly get => Bg.HighValue;
		set => Bg.HighValue = value;
	}

	public byte A {
		readonly get => Bg.LowValue;
		set => Bg.LowValue = value;
	}

	public readonly void GetChannels(out byte r, out byte g, out byte b, out byte a) => DefaultColorMethods.GetChannels(this, out r, out g, out b, out a);

	public void SetChannels(byte r, byte g, byte b, byte a) => DefaultColorMethods.SetChannels(ref this, r, g, b, a);

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => true;
	static bool IColor.HasBlueChannel => true;
	static bool IColor.HasAlphaChannel => true;
	static bool IColor.ChannelsAreFullyUtilized => false;
	static Type IColor.ChannelType => typeof(byte);
	static ChannelLayout IColor.ChannelLayout => ChannelLayout.BlueFirst;

	public static ColorB4G4R4A4 Black => new() { A = 0xF };
	public static ColorB4G4R4A4 White => new() { R = 0xF, G = 0xF, B = 0xF, A = 0xF };
	public static ColorB4G4R4A4 Transparent => Black with { A = 0 };

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B}, A: {A} }}";
}
