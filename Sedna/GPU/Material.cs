// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using Pluto.IO.Binary;
using SDL;
using static SDL.SDL3;

namespace Sedna.GPU;

public class Material : ManagedResource<MaterialResourceId> {
	internal Material(MaterialResourceId value, ResourceManager manager) : base(value, manager) { }

	public string? Name { get; set; }
	public LayerMask LayerMask { get; set; }
	public CullMode CullMode { get; set; }
	public IRentedArray<byte>? UniformBuffer { get; set; }
	public Dictionary<int, TextureResourceId> Textures { get; set; } = [];
	public ShaderResourceId VertexShader { get; set; }
	public ShaderResourceId FragmentShader { get; set; }

	public MemoryHandle UniformBufferHandle { get; set; }
	public unsafe SDL_GPUBuffer* DeviceUniformBuffer { get; set; }

	public override unsafe void Destroy() {
		if (UniformBufferHandle.Pointer != null) {
			UniformBufferHandle.Dispose();
			UniformBufferHandle = default;
		}

		if (DeviceUniformBuffer != null) {
			SDL_ReleaseGPUBuffer(Manager.Scene.Renderer.DeviceHandle, DeviceUniformBuffer);
			DeviceUniformBuffer = null;
		}
	}
}
