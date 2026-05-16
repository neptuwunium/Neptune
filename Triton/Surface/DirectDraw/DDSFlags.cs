// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton.Surface.DirectDraw;

// commented is old DirectDraw flags
[Flags]
public enum DDSFlags : uint {
	None = 0x0,
	Caps = 0x1,
	Height = 0x2,
	Width = 0x4,
	Pitch = 0x8,

	// BackBufferCount = 0x20,
	// ZBufferBitDepth = 0x40,
	// AlphaBitDepth = 0x80,
	// LPSurface = 0x800,
	PixelFormat = 0x1000,

	// CKDestinationOverlay = 0x2000,
	// CKDestinationBlt = 0x4000,
	// CKSourceOverlay = 0x8000,
	// CKSrcBlt = 0x10000,
	MipmapCount = 0x20000,

	// RefreshRate = 0x40000,
	LinearSize = 0x80000,

	// TextureStage = 0x100000,
	// FVF = 0x200000,
	// SourceVBHandle = 0x400000,
	Depth = 0x800000,
}
