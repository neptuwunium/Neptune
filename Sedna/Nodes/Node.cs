// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Silk.NET.Maths;

namespace Sedna.Nodes;

public class Node {
	public bool Enabled { get; set; }
	public string? Name { get; set; }
	public Node? ParentNode { get; set; }
	public List<Node> Children { get; set; } = [];
	public Matrix4X4<float> WorldTransform { get; set; }
	public Matrix4X4<float> LocalTransform { get; set; }

	public void AddChild(Node node) {
		node.ParentNode = this;
		Children.Add(node);
	}
}
