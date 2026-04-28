// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Sedna.Nodes;

public class MeshNode : Node {
	public MeshResourceId Mesh { get; set; }
	public List<MaterialResourceId> Materials { get; set; } = [];
}
