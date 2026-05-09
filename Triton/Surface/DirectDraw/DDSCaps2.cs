// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton.Surface.DirectDraw;

// commented is old DirectDraw flags
[Flags]
public enum DDSCaps2 : uint {
	None = 0x0,

	// Certified = 0x1,
	// HardwareDeinterlace = 0x2,
	// HintDynamic = 0x4,
	// HintStatic = 0x8,
	// TextureManage = 0x10,
	// Reserved1 = 0x20,
	// Reserved2 = 0x40,
	// Opaque = 0x80,
	// HintAntiAliasing = 0x100,

	Cubemap = 0x200,
	CubemapPositiveX = 0x400,
	CubemapNegativeX = 0x800,
	CubemapPositiveY = 0x1000,
	CubemapNegativeY = 0x2000,
	CubemapPositiveZ = 0x4000,
	CubemapNegativeZ = 0x8000,

	// MipmapSubLevel = 0x10000,
	// D3DTextureManage = 0x20000,
	// DoNotPersist = 0x40000,
	// StereoSurfaceLeft = 0x80000,

	Volume = 0x200000,

	// NotUserLockable = 0x400000,
	// Points = 0x800000,
	// RuntimePatches = 0x1000000,
	// NPatches = 0x2000000,
	// Reserved3 = 0x4000000,
	// DiscardBackBuffer = 0x10000000,
	// EnableAlphaChannel = 0x20000000,
	// ExtendedFormatPrimary = 0x40000000,
	// AdditionalPrimary = 0x80000000,

	CubemapAllFaces = Cubemap |
		CubemapPositiveX |
		CubemapNegativeX |
		CubemapPositiveY |
		CubemapNegativeY |
		CubemapPositiveZ |
		CubemapNegativeZ,
}
