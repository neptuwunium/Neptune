// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Formats;

/// <summary>
///     Also called RGB 565
/// </summary>
public record struct ColorRGB16 : IColor<ColorRGB16, byte> {
	private ushort Bits;

	/// <summary>
	///     5 bits
	/// </summary>
	public byte R {
		readonly get => (byte) (((uint) Bits >> 8) & 0xF8);
		set => Bits = (ushort) ((((uint) value << 8) & 0xF800u) | (Bits & ~0xF800u));
	}

	/// <summary>
	///     6 bits
	/// </summary>
	public byte G {
		readonly get => (byte) (((uint) Bits >> 3) & 0xFC);
		set => Bits = (ushort) ((((uint) value << 3) & 0x07E0u) | (Bits & ~0x07E0u));
	}

	/// <summary>
	///     5 bits
	/// </summary>
	public byte B {
		readonly get => (byte) (((uint) Bits << 3) & 0xF8);
		set => Bits = (ushort) ((((uint) value >> 3) & 0x001Fu) | (Bits & ~0x001Fu));
	}

	public readonly byte A {
		get => byte.MaxValue;
		set { }
	}

	public readonly void GetChannels(out byte r, out byte g, out byte b, out byte a) => DefaultColorMethods.GetChannels(this, out r, out g, out b, out a);

	public void SetChannels(byte r, byte g, byte b, byte a) => Bits = (ushort) (((r & 0xF8u) << 8) | ((g & 0xFCu) << 3) | ((b & 0xF8u) >> 3));

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => true;
	static bool IColor.HasBlueChannel => true;
	static bool IColor.HasAlphaChannel => false;
	static bool IColor.ChannelsAreFullyUtilized => false;
	static Type IColor.ChannelType => typeof(byte);
	public static ColorRGB16 Black => new();
	public static ColorRGB16 White => new() { R = 0xF8, G = 0xFC, B = 0xF8 };
	public static ColorRGB16 Transparent => Black;

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B} }}";
}
