// SPDX-FileCopyrightText: 2022 - 2026 ds5678
// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Formats;

public record struct ColorA4R4G4B4 : IColor<ColorA4R4G4B4, byte> {
	private UInt4Pair Ar;
	private UInt4Pair Gb;

	public byte B {
		readonly get => Gb.LowValue;
		set => Gb.LowValue = value;
	}

	public byte G {
		readonly get => Gb.HighValue;
		set => Gb.HighValue = value;
	}

	public byte R {
		readonly get => Ar.LowValue;
		set => Ar.LowValue = value;
	}

	public byte A {
		readonly get => Ar.HighValue;
		set => Ar.HighValue = value;
	}

	public readonly void GetChannels(out byte r, out byte g, out byte b, out byte a) => DefaultColorMethods.GetChannels(this, out r, out g, out b, out a);

	public void SetChannels(byte r, byte g, byte b, byte a) => DefaultColorMethods.SetChannels(ref this, r, g, b, a);

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => true;
	static bool IColor.HasBlueChannel => true;
	static bool IColor.HasAlphaChannel => true;
	static bool IColor.ChannelsAreFullyUtilized => false;
	static Type IColor.ChannelType => typeof(byte);
	static ChannelLayout IColor.ChannelLayout => ChannelLayout.AlphaRedFirst;
	public static ColorA4R4G4B4 Black => new() { A = 0xF };
	public static ColorA4R4G4B4 White => new() { R = 0xF, G = 0xF, B = 0xF, A = 0xF };
	public static ColorA4R4G4B4 Transparent => Black with { A = 0 };

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B}, A: {A} }}";
}
