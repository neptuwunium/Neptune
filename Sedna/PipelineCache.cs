// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Charon.Hash.Algorithms;
using Pluto.Extensions;
using SDL;
using Sedna.GPU;
using static SDL.SDL3;

namespace Sedna;

public class PipelineCache {
	public Dictionary<UInt128, nint> PipelineObjects { get; } = [];
	public Dictionary<ulong, HashSet<UInt128>> MaterialToPipelines { get; } = [];

	public static UInt128 CreatePipelineId(Mesh mesh, Material material) {
		var meshHash = mesh.PipelineHash.Value;
		var materialHash = material.PipelineHash.Value;

		Span<ulong> hash = stackalloc ulong[4];
		hash[0] = meshHash.Low;
		hash[1] = materialHash.Low;
		hash[2] = meshHash.High;
		hash[3] = materialHash.High;

		var (lo, hi) = MurmurHash3Algorithm.Hash64_128(MemoryMarshal.AsBytes(hash));
		
		return new UInt128(hi, lo);
	}

	public unsafe SDL_GPUGraphicsPipeline* Create(ResourceManager manager, Mesh mesh, Material material) {
		var pipelineId = CreatePipelineId(mesh, material);

		if (PipelineObjects.TryGetValue(pipelineId, out var ptr)) {
			return (SDL_GPUGraphicsPipeline*) ptr;
		}

		if (mesh.VertexStrides.Count != mesh.VertexOffsets.Count) {
			// todo logging
			return null;
		}

		var vertShader = manager.Find<Shader, ShaderResourceId>(material.VertexShader);
		var fragShader = manager.Find<Shader, ShaderResourceId>(material.FragmentShader);

		if (vertShader == null || fragShader == null) {
			// todo logging
			return null;
		}

		var device = manager.Scene.Renderer.DeviceHandle;
		var window = manager.Scene.Renderer.WindowHandle;

		vertShader.Create();
		fragShader.Create();

		var vattr = stackalloc SDL_GPUVertexAttribute[mesh.Semantics.Count];
		Span<uint> bufferIdx = stackalloc uint[mesh.Semantics.Count];
		for (var index = 0; index < mesh.Semantics.Count; index++) {
			var attribute = mesh.Semantics[index];
			vattr[index] = new SDL_GPUVertexAttribute {
				location = bufferIdx[attribute.BufferIndex]++,
				offset = (uint) attribute.Offset,
				buffer_slot = (uint) attribute.BufferIndex,
				format = attribute.Format,
			};
		}

		var vbind = stackalloc SDL_GPUVertexBufferDescription[mesh.VertexStrides.Count];
		for (var index = 0; index < mesh.VertexStrides.Count; index++) {
			var stride = mesh.VertexStrides[index];
			vbind[index] = new SDL_GPUVertexBufferDescription {
				slot = (uint) index,
				pitch = (uint) stride,
				input_rate = SDL_GPUVertexInputRate.SDL_GPU_VERTEXINPUTRATE_VERTEX,
				instance_step_rate = 0,
			};
		}

		// todo: pbr targets instead of swap for deferred opaque
		// 3 targets: color + ao, normal + emission, metalness + roughness 
		// + depth
		var colorTargetDesc = new SDL_GPUColorTargetDescription {
			format = SDL_GetGPUSwapchainTextureFormat(device, window),
		};

		var pipelineInfo = new SDL_GPUGraphicsPipelineCreateInfo {
			vertex_shader = vertShader.DeviceShader,
			fragment_shader = fragShader.DeviceShader,
			vertex_input_state = new SDL_GPUVertexInputState {
				vertex_attributes = vattr,
				num_vertex_attributes = (uint) mesh.Semantics.Count,
				vertex_buffer_descriptions = vbind,
				num_vertex_buffers = (uint) mesh.VertexStrides.Count,
			},
			primitive_type = mesh.Type,
			rasterizer_state = new SDL_GPURasterizerState { cull_mode = material.CullMode },
			target_info = new SDL_GPUGraphicsPipelineTargetInfo {
				color_target_descriptions = &colorTargetDesc,
				num_color_targets = 1,
			},
		};

		var pipeline = SDL_CreateGPUGraphicsPipeline(device, &pipelineInfo);
		PipelineObjects[pipelineId] = (nint) pipeline;
		if (!MaterialToPipelines.TryGetValue(material.Id, out var materialPipelines)) {
			materialPipelines = MaterialToPipelines[material.Id] = [];
		}

		materialPipelines.Add(pipelineId);


		return pipeline;
	}

	public unsafe void Destroy(ResourceManager manager, Mesh mesh, Material material) {
		var pipelineId = CreatePipelineId(mesh, material);

		if (PipelineObjects.Remove(pipelineId, out var ptr)) {
			SDL_ReleaseGPUGraphicsPipeline(manager.Scene.Renderer.DeviceHandle, (SDL_GPUGraphicsPipeline*) ptr);
		}

		if (MaterialToPipelines.TryGetValue(material.Id, out var materialPipelines)) {
			materialPipelines.Remove(pipelineId);
		}
	}

	public unsafe void Destroy(ResourceManager manager, Material material) {
		if (!MaterialToPipelines.TryGetValue(material.Id, out var materialPipelines)) {
			return;
		}

		var device = manager.Scene.Renderer.DeviceHandle;
		foreach (var pipelineId in materialPipelines) {
			if (!PipelineObjects.Remove(pipelineId, out var ptr)) {
				continue;
			}

			SDL_ReleaseGPUGraphicsPipeline(device, (SDL_GPUGraphicsPipeline*) ptr);
		}
		
		materialPipelines.Clear();
	}
}
