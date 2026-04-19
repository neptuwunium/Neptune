// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

namespace Sedna.GPU;

[Flags]
public enum LayerMask {
	ShadowCaster = 1,
	DeferredOpaque = 2,
	ForwardOpaque = 4,
	Transparent = 8,
}
