// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Charon.Hash.Algorithms;
using Sedna.GPU;

namespace Sedna;

public class ResourceManager {
	public Dictionary<TextureResourceId, Texture> Textures { get; } = [];
	public Dictionary<ShaderResourceId, Shader> Shaders { get; } = [];
	public Dictionary<MeshResourceId, Mesh> Meshes { get; } = [];
	public Dictionary<MaterialResourceId, Material> Materials { get; } = [];
	
	public SednaScene Scene { get; }

	internal ResourceManager(SednaScene scene) => Scene = scene;

	public static ulong CreateId(string name, string? tweak = null) => CityHashAlgorithm.Hash64(name) ^ (tweak != null ? CityHashAlgorithm.Hash64(tweak) : 0);

	private static ulong CreateId(string name, ResourceKind kind) {
		var value = CityHashAlgorithm.Hash64(name);
		return CreateId(value, kind);
	}

	private static ulong CreateId(ulong value, ResourceKind kind) {
		if (value >> 56 == (ulong) kind) {
			return value;
		}

		value &= 0x00FFFFFFFFFFFFFFu;
		value |= (ulong) kind << 56;
		return value;
	}

	// todo: T -> Texture1D, Texture2D, Texture3D, TextureArray, TextureCube?
	public Texture CreateTexture(string name, ulong? id = null) {
		TextureResourceId textureId = id.HasValue ? CreateId(id.Value, ResourceKind.Texture) : CreateId(name, ResourceKind.Texture);
		if (Textures.TryGetValue(textureId, out var texture)) {
			return texture;
		}

		return Textures[textureId] = new Texture(textureId, this);
	}

	public T CreateShader<T>(string name, ulong? id = null) where T : Shader {
		ShaderResourceId shaderId = id.HasValue ? CreateId(id.Value, ResourceKind.Shader) : CreateId(name, ResourceKind.Shader);
		if (Shaders.TryGetValue(shaderId, out var shader)) {
			return shader as T ?? throw new InvalidDataException();
		}

		var instance = Activator.CreateInstance(typeof(T), shaderId, this);
		if (instance is not T tInstance) {
			throw new InvalidOperationException();
		}

		Shaders[shaderId] = tInstance;
		return tInstance;
	}

	public Material CreateMaterial(string name, ulong? id = null) {
		MaterialResourceId materialId = id.HasValue ? CreateId(id.Value, ResourceKind.Material) : CreateId(name, ResourceKind.Material);
		if (Materials.TryGetValue(materialId, out var material)) {
			return material;
		}

		return Materials[materialId] = new Material(materialId, this);
	}

	// todo: T -> Mesh, SkinnedMesh
	public Mesh CreateMesh(string name, ulong? id = null) {
		MeshResourceId meshId = id.HasValue ? CreateId(id.Value, ResourceKind.Mesh) : CreateId(name, ResourceKind.Mesh);
		if (Meshes.TryGetValue(meshId, out var mesh)) {
			return mesh;
		}

		return Meshes[meshId] = new Mesh(meshId, this);
	}

	private void Destroy<T, TId>(Dictionary<TId, T> values, TId id) where TId : struct, IResourceId where T : ManagedResource<TId> {
		// todo: wait on device fence maybe?

		if (values.Remove(id, out var value)) {
			value.Destroy();
		}
	}

	public void Destroy(IResourceId id) {
		switch (id.Kind) {
			case ResourceKind.Texture: {
				Destroy(Textures, id.Value);
				break;
			}
			case ResourceKind.Shader: {
				Destroy(Shaders, id.Value);
				break;
			}
			case ResourceKind.Mesh: {
				Destroy(Meshes, id.Value);
				break;
			}
			case ResourceKind.Material: {
				Destroy(Materials, id.Value);
				break;
			}
			case ResourceKind.Invalid:
			default: throw new ArgumentOutOfRangeException(nameof(id));
		}
	}
}
