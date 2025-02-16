// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Formats;

public record struct ColorARGB16 : IColor<byte> {
	private UInt4Pair ar;
	private UInt4Pair gb;

	public byte B {
		readonly get => gb.LowValue;
		set => gb.LowValue = value;
	}

	public byte G {
		readonly get => gb.HighValue;
		set => gb.HighValue = value;
	}

	public byte R {
		readonly get => ar.LowValue;
		set => ar.LowValue = value;
	}

	public byte A {
		readonly get => ar.HighValue;
		set => ar.HighValue = value;
	}

	public readonly void GetChannels(out byte r, out byte g, out byte b, out byte a) {
		DefaultColorMethods.GetChannels(this, out r, out g, out b, out a);
	}

	public void SetChannels(byte r, byte g, byte b, byte a) {
		DefaultColorMethods.SetChannels(ref this, r, g, b, a);
	}

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => true;
	static bool IColor.HasBlueChannel => true;
	static bool IColor.HasAlphaChannel => true;
	static bool IColor.ChannelsAreFullyUtilized => false;
	static Type IColor.ChannelType => typeof(byte);

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B}, A: {A} }}";
}
