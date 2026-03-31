// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Silk.NET.Maths;

namespace Sedna.Nodes;

public class InstancedMeshNode : MeshNode {
	public List<Matrix4X4<float>> Instances { get; set; } = [];
}
