// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton.Surface.DirectDraw;

// commented is old DirectDraw flags
[Flags]
public enum DDSPixelFlags {
	None = 0x0,
	AlphaPixels = 0x1,
	Alpha = 0x2,
	FourCC = 0x4,
	// PaletteIndexed4 = 0x8,
	// PaletteIndexedTo8 = 0x10,
	// PaletteIndexed8 = 0x20,
	RGB = 0x40,
	// Compressed = 0x80,
	// RGBToYUV = 0x100,
	YUV = 0x200,
	// ZBuffer = 0x400,
	// PaletteIndexed1 = 0x800,
	// PaletteIndexed2 = 0x1000,
	// ZPixels = 0x2000,
	// StencilBuffer = 0x4000,
	// AlphaPremultiplied = 0x8000,
	Luminance = 0x20000,
	// BumpLuminance = 0x40000,
	// BumpDUDV = 0x80000,
}
