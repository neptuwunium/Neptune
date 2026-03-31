// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Sedna.Geometry;

namespace Sedna.Nodes;

public class MeshNode : Node {
	public Mesh? Mesh { get; set; }
	public List<MaterialInstance?> Materials { get; set; } = [];
}
