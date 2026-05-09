// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton.Surface.DirectDraw;

// commented is old DirectDraw flags
[Flags]
public enum DDSCaps1 : uint {
	None = 0x0,
	// Reserved1 = 0x1,
	// Alpha = 0x2,
	// Backbuffer = 0x4,
	Complex = 0x8,
	// Flip = 0x10,
	// FrontBuffer = 0x20,
	// OffscreenPlain = 0x40,
	// Overlay = 0x80,
	// Palette = 0x100,
	// PrimarySurface = 0x200,
	// PrimarySurfaceLeft = 0x400,
	// SystemMemory = 0x800,
	Texture = 0x1000,
	// ThreedDevice = 0x2000,
	// VideoMemory = 0x4000,
	// Visible = 0x8000,
	// WriteOnly = 0x10000,
	// ZBuffer = 0x20000,
	// OwnDC = 0x40000,
	// LiveVideo = 0x80000,
	// HardwareCodec = 0x100000,
	// ModeX = 0x200000,
	Mipmap = 0x400000,
	// Reserved2 = 0x800000,
	// AllocOnLoad = 0x4000000,
	// VideoPort = 0x8000000,
	// LocalVideoMemory = 0x10000000,
	// NonLocalVideoMemory = 0x20000000,
	// StandardVGAMode = 0x40000000,
	// Optimized = 0x80000000,
}
