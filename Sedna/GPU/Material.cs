// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Charon.Hash.Algorithms;
using Pluto.IO.Binary;
using SDL;
using static SDL.SDL3;

namespace Sedna.GPU;

public class Material : ManagedResource<MaterialResourceId> {
	internal Material(MaterialResourceId value, ResourceManager manager) : base(value, manager) { }

	public string? Name { get; set; }
	public LayerMask LayerMask { get; set; }
	public SDL_GPUCullMode CullMode { get; set; }
	public IRentedArray<byte>? UniformBuffer { get; set; }
	public Dictionary<int, TextureResourceId> Textures { get; set; } = [];
	public ShaderResourceId VertexShader { get; set; }
	public ShaderResourceId FragmentShader { get; set; }

	public unsafe SDL_GPUBuffer* DeviceUniformBuffer { get; set; }

	public PipelineId PipelineHash {
		get {
			if (field == default(PipelineId)) {
				field = CreatePipelineHash();
			}

			return field;
		}
		set {
			if (field != default(PipelineId)) {
				Manager.Scene.PipelineCache.Destroy(Manager, this);
			}

			field = value;
		}
	}

	public void Invalidate() => PipelineHash = default;

	public PipelineId CreatePipelineHash() {
		using var writer = new ArrayPoolBinaryWriter();

		writer.Write((int) CullMode);
		writer.Write((int) LayerMask);

		return CityHashAlgorithm.Hash128(writer.Array.AsSpan(0, writer.Length));
	}

	public override unsafe void Create() {
		if (DeviceUniformBuffer != null) {
			return;
		}

		if (UniformBuffer == null) {
			// todo logging
			return;
		}

		if (UniformBuffer.Length % 16 > 0) {
			// todo logging
			return;
		}

		var renderer = Manager.Scene.Renderer;
		var device = renderer.DeviceHandle;

		var cmd = SDL_AcquireGPUCommandBuffer(device);
		var pass = SDL_BeginGPUCopyPass(cmd);
		var (buffer, transfer) = renderer.UploadBuffer(UniformBuffer, SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_GRAPHICS_STORAGE_READ, pass);
		SDL_EndGPUCopyPass(pass);
		SDL_SubmitGPUCommandBuffer(cmd);
		SDL_ReleaseGPUTransferBuffer(device, (SDL_GPUTransferBuffer*) transfer);
		DeviceUniformBuffer = (SDL_GPUBuffer*) buffer;
	}

	public override unsafe void Destroy() {
		if (DeviceUniformBuffer != null) {
			SDL_ReleaseGPUBuffer(Manager.Scene.Renderer.DeviceHandle, DeviceUniformBuffer);
			DeviceUniformBuffer = null;
		}

		Manager.Scene.PipelineCache.Destroy(Manager, this);
	}
}
