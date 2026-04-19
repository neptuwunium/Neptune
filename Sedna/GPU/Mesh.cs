// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

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
	public List<int> VertexOffsets { get; set; } = [];
	public SDL_GPUPrimitiveType Type { get; set; } = SDL_GPUPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLELIST;
	public int VertexCount { get; set; }
	public int IndexCount { get; set; }
	public ComponentType IndexType { get; set; }
	public List<SubMesh> SubMeshes { get; set; } = [];

	public unsafe SDL_GPUBuffer* DeviceVertexBuffer { get; set; }
	public unsafe SDL_GPUBuffer* DeviceIndexBuffer { get; set; }

	public override unsafe void Create() {
		if (DeviceVertexBuffer != null) {
			return;
		}

		if (VertexBuffer == null || VertexCount == 0 ||
			VertexStrides.Count == 0 || VertexOffsets.Count == 0) {
			// todo logging
			return;
		}

		if (VertexStrides.Count != VertexOffsets.Count) {
			// todo logging
			return;
		}

		var renderer = Manager.Scene.Renderer;
		var device = renderer.DeviceHandle;

		var cmd = SDL_AcquireGPUCommandBuffer(device);
		var pass = SDL_BeginGPUCopyPass(cmd);
		var (vertexBuffer, vertexTransfer) = renderer.UploadBuffer(VertexBuffer, SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_VERTEX, pass);
		var (indexBuffer, indexTransfer) = renderer.UploadBuffer(IndexBuffer, SDL_GPUBufferUsageFlags.SDL_GPU_BUFFERUSAGE_INDEX, pass);
		SDL_EndGPUCopyPass(pass);
		SDL_SubmitGPUCommandBuffer(cmd);
		SDL_ReleaseGPUTransferBuffer(device, (SDL_GPUTransferBuffer*) vertexTransfer);
		if (indexTransfer != nint.Zero) {
			SDL_ReleaseGPUTransferBuffer(device, (SDL_GPUTransferBuffer*) indexTransfer);
		}

		DeviceVertexBuffer = (SDL_GPUBuffer*) vertexBuffer;
		DeviceIndexBuffer = (SDL_GPUBuffer*) indexBuffer;
	}

	public unsafe SDL_GPUGraphicsPipeline* CreatePipeline(Material material) {
		Create();

		if (DeviceVertexBuffer == null) {
			return null;
		}

		if (VertexStrides.Count != VertexOffsets.Count) {
			// todo logging
			return null;
		}

		// todo: Create Manager.PipelineCache.Create(material, mesh). This is only here because VertexSemantics is on the mesh.
		var vertShader = Manager.Find<Shader, ShaderResourceId>(material.VertexShader);
		var fragShader = Manager.Find<Shader, ShaderResourceId>(material.FragmentShader);

		if (vertShader == null || fragShader == null) {
			// todo logging
			return null;
		}

		var device = Manager.Scene.Renderer.DeviceHandle;
		var window = Manager.Scene.Renderer.WindowHandle;

		vertShader.Create();
		fragShader.Create();

		var vattr = stackalloc SDL_GPUVertexAttribute[Semantics.Count];
		Span<uint> bufferIdx = stackalloc uint[Semantics.Count];
		for (var index = 0; index < Semantics.Count; index++) {
			var attribute = Semantics[index];
			vattr[index] = new SDL_GPUVertexAttribute {
				location = bufferIdx[attribute.BufferIndex]++,
				offset = (uint) attribute.Offset,
				buffer_slot = (uint) attribute.BufferIndex,
				format = attribute.Format,
			};
		}

		var vbind = stackalloc SDL_GPUVertexBufferDescription[VertexStrides.Count];
		for (var index = 0; index < VertexStrides.Count; index++) {
			var stride = VertexStrides[index];
			vbind[index] = new SDL_GPUVertexBufferDescription {
				slot = (uint) index,
				pitch = (uint) stride,
				input_rate = SDL_GPUVertexInputRate.SDL_GPU_VERTEXINPUTRATE_VERTEX,
				instance_step_rate = 0,
			};
		}

		// todo: pbr targets instead of swap for opaque
		// 3 targets: color + ao, normal + emission, metalness + roughness 
		var colorTargetDesc = new SDL_GPUColorTargetDescription {
			format = SDL_GetGPUSwapchainTextureFormat(device, window),
		};

		var pipelineInfo = new SDL_GPUGraphicsPipelineCreateInfo {
			vertex_shader = vertShader.DeviceShader,
			fragment_shader = fragShader.DeviceShader,
			vertex_input_state = new SDL_GPUVertexInputState {
				vertex_attributes = vattr,
				num_vertex_attributes = (uint) Semantics.Count,
				vertex_buffer_descriptions = vbind,
				num_vertex_buffers = (uint) VertexStrides.Count,
			},
			primitive_type = Type,
			rasterizer_state = new SDL_GPURasterizerState { cull_mode = material.CullMode },
			target_info = new SDL_GPUGraphicsPipelineTargetInfo {
				color_target_descriptions = &colorTargetDesc,
				num_color_targets = 1,
			},
		};

		return SDL_CreateGPUGraphicsPipeline(device, &pipelineInfo);
	}

	public Dictionary<MaterialResourceId, List<SubMesh>>? CollectSubmeshes(List<MaterialResourceId> resourceIds) {
		if (resourceIds.Count != SubMeshes.Count) {
			// todo logging
			return null;
		}

		var result = new Dictionary<MaterialResourceId, List<SubMesh>>();
		foreach (var (subMesh, id) in SubMeshes.Zip(resourceIds)) {
			if (!result.TryGetValue(id, out var entries)) {
				entries = result[id] = [];
			}

			entries.Add(subMesh);
		}

		return result;
	}

	public unsafe void Draw(SDL_GPURenderPass* pass, SDL_GPUDevice* device, List<SubMesh> submeshForMaterial, uint firstSlot = 0) {
		if (VertexStrides.Count != VertexOffsets.Count) {
			// todo logging
			throw new InvalidOperationException();
		}

		var bindings = stackalloc SDL_GPUBufferBinding[VertexOffsets.Count];
		for (var index = 0; index < VertexOffsets.Count; index++) {
			bindings[index] = new SDL_GPUBufferBinding { buffer = DeviceVertexBuffer, offset = (uint) VertexOffsets[index] };
		}

		SDL_BindGPUVertexBuffers(pass, firstSlot, bindings, (uint) VertexOffsets.Count);

		if (DeviceIndexBuffer != null) {
			var indexBinding = new SDL_GPUBufferBinding { buffer = DeviceIndexBuffer, offset = 0 };
			SDL_BindGPUIndexBuffer(pass, &indexBinding, IndexType switch {
				ComponentType.Int or ComponentType.UInt => SDL_GPUIndexElementSize.SDL_GPU_INDEXELEMENTSIZE_32BIT,
				ComponentType.Short or ComponentType.UShort => SDL_GPUIndexElementSize.SDL_GPU_INDEXELEMENTSIZE_16BIT,
				_ => throw new InvalidOperationException(),
			});

			foreach (var submesh in submeshForMaterial) {
				SDL_DrawGPUIndexedPrimitives(pass, (uint) submesh.Count, 1, (uint) submesh.FirstIndex, submesh.FirstVertex, 0);
			}
		} else {
			foreach (var submesh in submeshForMaterial) {
				SDL_DrawGPUPrimitives(pass, (uint) submesh.Count, 1, (uint) submesh.FirstVertex, 0);
			}
		}
	}

	public override unsafe void Destroy() {
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
