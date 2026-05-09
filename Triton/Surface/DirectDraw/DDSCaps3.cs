// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton.Surface.DirectDraw;

// commented is old DirectDraw flags
[Flags]
public enum DDSCaps3 : uint {
	None = 0,

	// MultiSampleMask = 0x1F,
	// MultiSampleQualityMask = 0x000000E0,
	// Reserved1 = 0x100,
	// Reserved2 = 0x200,
	// LightWeightMipmap = 0x400,
	// AutogenMipmap = 0x800,
	// DMap = 0x1000,
	// CreateSharedResource = 0x2000,
	// ReadOnlyResource = 0x4000,
	// OpenSharedResource = 0x8000,
}
