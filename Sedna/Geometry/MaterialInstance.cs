// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Charon.Hash.Algorithms;
using Pluto.IO.Binary;

namespace Sedna.Geometry;

public class MaterialInstance {
	public ulong MaterialId { get; private set; }
	
	public required string Name {
		get;
		set {
			field = value;
			MaterialId = CityHashAlgorithm.Hash64(value);
		}
	}

	public LayerMask LayerMask { get; set; }
	public CullMode CullMode { get; set; }
	public IRentedArray<byte>? UniformBuffer { get; set; }
	public List<Texture> Textures { get; set; } = [];
	public string? VertexShader { get; set; }
	public string? FragmentShader { get; set; }
}
