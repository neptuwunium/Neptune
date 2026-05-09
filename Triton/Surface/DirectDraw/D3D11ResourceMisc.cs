// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton.Surface.DirectDraw;

// commented is dx11 flags
[Flags]
public enum D3D11ResourceMisc : uint {
	None = 0x0,

	// GenerateMips = 0x1,
	// Shared = 0x2,
	TextureCube = 0x4,
	// DrawIndirectArgs = 0x10,
	// BufferAllowRawViews = 0x20,
	// BufferStructured = 0x40,
	// ResourceClamp = 0x80,
	// SharedKeyedMutex = 0x100,
	// GdiCompatible = 0x200,
	// SharedNTHandle = 0x800,
	// RestrictedContent = 0x1000,
	// RestrictSharedResource = 0x2000,
	// RestrictSharedResourceDriver = 0x4000,
	// Guarded = 0x8000,
	// TilePool = 0x20000,
	// Tiled = 0x40000,
	// HardwareProtected = 0x80000,
	// SharedDisplayable = 0x100000,
	// SharedExclusiveWriter = 0x200000,
	// NoShaderAccess = 0x400000,
}
