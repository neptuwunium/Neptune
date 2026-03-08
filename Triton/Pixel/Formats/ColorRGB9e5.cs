// SPDX-FileCopyrightText: 2022 - 2026 ds5678
// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;

namespace Triton.Pixel.Formats;

/// <summary>
///     9 bits each for RGB and 5 bits for an exponent
/// </summary>
public record struct ColorRGB9e5 : IColor<ColorRGB9e5, double> {
	private const int ChannelBitMask = 0x1FF;
	private const int RedOffset = 0;
	private const int GreenOffset = 9;
	private const int BlueOffset = 18;
	private const int ExponentOffset = 27;
	private uint Bits;

	/// <summary>
	///     Range: -24 to 7 inclusive
	/// </summary>
	private readonly int Exponent => unchecked((int) (Bits >> ExponentOffset) - 24);

	private readonly double Scale => double.Pow(2, Exponent);
	private readonly uint RBits => (Bits >> RedOffset) & ChannelBitMask;
	private readonly uint GBits => (Bits >> GreenOffset) & ChannelBitMask;
	private readonly uint BBits => (Bits >> BlueOffset) & ChannelBitMask;

	public double R {
		readonly get => RBits * Scale;
		set {
			GetChannels(out _, out var g, out var b, out _);
			SetChannels(value, g, b);
		}
	}

	public double G {
		readonly get => GBits * Scale;
		set {
			GetChannels(out var r, out _, out var b, out _);
			SetChannels(r, value, b);
		}
	}

	public double B {
		readonly get => BBits * Scale;
		set {
			GetChannels(out var r, out var g, out _, out _);
			SetChannels(r, g, value);
		}
	}

	public readonly double A {
		get => 1;
		set { }
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly void GetChannels(out double r, out double g, out double b, out double a) {
		var scale = Scale;
		r = RBits * scale;
		g = GBits * scale;
		b = BBits * scale;
		a = 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void SetChannels(double r, double g, double b, double a) => SetChannels(r, g, b);

	static bool IColor.HasRedChannel => true;
	static bool IColor.HasGreenChannel => true;
	static bool IColor.HasBlueChannel => true;
	static bool IColor.HasAlphaChannel => false;
	static bool IColor.ChannelsAreFullyUtilized => false;
	static Type IColor.ChannelType => typeof(double);
	static ChannelLayout IColor.ChannelLayout => ChannelLayout.RedFirst;
	public static ColorRGB9e5 Black => new();

	public static ColorRGB9e5 White {
		get {
			var color = new ColorRGB9e5();
			color.SetChannels(1.0, 1.0, 1.0);
			return color;
		}
	}

	public static ColorRGB9e5 Transparent => Black;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void SetChannels(double r, double g, double b) {
		var exponent = CalculateExponent(r, g, b);
		var scale = double.Pow(2, exponent);
		var rBits = (uint) (r / scale) & ChannelBitMask;
		var gBits = (uint) (g / scale) & ChannelBitMask;
		var bBits = (uint) (b / scale) & ChannelBitMask;
		var exponentBits = unchecked((uint) (exponent + 24));
		Bits = (exponentBits << ExponentOffset) | (bBits << BlueOffset) | (gBits << GreenOffset) | (rBits << RedOffset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static int CalculateExponent(double r, double g, double b) {
		var maxChannel = double.Max(r, double.Max(g, b));
		var minExponent = double.Log2(maxChannel / ChannelBitMask);
		return (int) double.Ceiling(minExponent);
	}

	public override string ToString() => $"{{ R: {R}, G: {G}, B: {B} }}";
}
