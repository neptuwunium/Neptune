// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Silk.NET.Maths;

namespace Sedna.Nodes;

public class InstancedMeshNode : MeshNode {
	public List<Matrix4X4<float>> Instances { get; set; } = [];
}
