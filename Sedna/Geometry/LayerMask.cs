// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

namespace Sedna.Geometry;

[Flags]
public enum LayerMask {
	ShadowCaster = 1,
	Opaque = 2,
	Transparent = 4,
}
