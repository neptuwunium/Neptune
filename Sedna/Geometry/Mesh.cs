// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Charon.Hash.Algorithms;
using Pluto.IO.Binary;

namespace Sedna.Geometry;

public class Mesh {
	public ulong MeshId { get; private set; }
	
	public required string Name {
		get;
		set {
			field = value;
			MeshId = CityHashAlgorithm.Hash64(value);
		}
	}
	
	public List<VertexSemanticInfo> Semantics { get; set; } = [];
	public IRentedArray<byte>? VertexBuffer { get; set; }
	public IRentedArray<byte>? IndexBuffer { get; set; }
	public List<int> VertexStrides { get; set; } = [];
	public int VertexCount { get; set; }
	public int IndexCount { get; set; }
	public ComponentType IndexType { get; set; }
	public List<SubMesh> SubMeshes { get; set; } = [];
}
