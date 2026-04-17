// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using Pluto.IO.Binary;
using SDL;
using static SDL.SDL3;

namespace Sedna.GPU;

public class Mesh : ManagedResource<MeshResourceId> {
	internal Mesh(MeshResourceId value, ResourceManager manager) : base(value, manager) { }
	
	public string? Name { get; set; }
	public List<VertexSemanticInfo> Semantics { get; set; } = [];
	public IRentedArray<byte>? VertexBuffer { get; set; }
	public IRentedArray<byte>? IndexBuffer { get; set; }
	public List<int> VertexStrides { get; set; } = [];
	public int VertexCount { get; set; }
	public int IndexCount { get; set; }
	public ComponentType IndexType { get; set; }
	public List<SubMesh> SubMeshes { get; set; } = [];

	public MemoryHandle VertexBufferHandle { get; set; }
	public MemoryHandle IndexBufferHandle { get; set; }
	public unsafe SDL_GPUBuffer* DeviceVertexBuffer { get; set; }
	public unsafe SDL_GPUBuffer* DeviceIndexBuffer { get; set; }

	public override unsafe void Destroy() {
		if (VertexBufferHandle.Pointer != null) {
			VertexBufferHandle.Dispose();
			VertexBufferHandle = default;
		}

		if (IndexBufferHandle.Pointer != null) {
			IndexBufferHandle.Dispose();
			IndexBufferHandle = default;
		}

		if (DeviceVertexBuffer != null) {
			SDL_ReleaseGPUBuffer(Manager.Scene.Renderer.DeviceHandle, DeviceVertexBuffer);
			DeviceVertexBuffer = null;
		}

		if (DeviceIndexBuffer != null) {
			SDL_ReleaseGPUBuffer(Manager.Scene.Renderer.DeviceHandle, DeviceIndexBuffer);
			DeviceIndexBuffer = null;
		}
	}
}
