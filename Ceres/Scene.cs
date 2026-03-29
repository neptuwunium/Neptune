// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;

namespace Ceres;

/// <summary>The root nodes of a scene.</summary>
public class Scene : ChildOfRootProperty, INodeCreator {
	/// <summary>The indices of each root node.</summary>
	[JsonPropertyName("nodes")]
	public List<int> Nodes { get; set; } = [];

	public (Node Node, int Id) CreateNode(Root root, string name) {
		var node = new Node {
			Name = name,
		};
		root.Nodes ??= [];
		var id = root.Nodes.Count;
		root.Nodes.Add(node);
		Nodes.Add(id);
		node.Id = id;
		return (node, id);
	}
}
