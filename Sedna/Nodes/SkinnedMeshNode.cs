// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Silk.NET.Maths;

namespace Sedna.Nodes;

public class SkinnedMeshNode : MeshNode {
	public List<Node> JointNodes { get; set; } = [];
	public List<Matrix4X4<float>> InverseBindMatrices { get; set; } = [];
}
