// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Formats;

public record struct ColorBGRA32 : IColor<byte> {
	public byte B { get; set; }
	public byte G { get; set; }
	public byte R { get; set; }
	public byte A { get; set; }

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
	static bool IColor.ChannelsAreFullyUtilized => true;
	static Type IColor.ChannelType => typeof(byte);

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B}, A: {A} }}";
}
