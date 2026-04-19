// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Pluto.IO.Binary;
using SDL;
using static SDL.SDL3;

namespace Sedna.GPU;

public class Texture : ManagedResource<TextureResourceId> {
	internal Texture(TextureResourceId value, ResourceManager manager) : base(value, manager) { }

	public string? Name { get; set; }
	public IRentedArray<byte>? Data { get; set; }
	public SDL_GPUTextureCreateInfo TextureInfo { get; set; } = new();
	public SDL_GPUSamplerCreateInfo SamplerInfo { get; set; } = new();

	public unsafe SDL_GPUSampler* DeviceSampler { get; set; }
	public unsafe SDL_GPUTexture* DeviceTexture { get; set; }

	public override unsafe void Create() {
		var device = Manager.Scene.Renderer.DeviceHandle;

		if (DeviceTexture != null) {
			if (Data == null) {
				// todo logging
				return;
			}

			var info = TextureInfo;
			DeviceTexture = SDL_CreateGPUTexture(device, &info);
			if (DeviceTexture == null) {
				// todo logging
				return;
			}

			var transferInfo = new SDL_GPUTransferBufferCreateInfo {
				usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
				size = (uint) Data.Length,
			};

			var transfer = SDL_CreateGPUTransferBuffer(device, &transferInfo);
			if (transfer == null) {
				Destroy();
				return;
			}

			var map = SDL_MapGPUTransferBuffer(device, transfer, false);
			if (map == nint.Zero) {
				SDL_ReleaseGPUTransferBuffer(device, transfer);
				Destroy();
				return;
			}

			using (var pinned = Data.Memory.Pin()) {
				Buffer.MemoryCopy(pinned.Pointer, (void*) map, Data.Length, Data.Length);
			}

			SDL_UnmapGPUTransferBuffer(device, transfer);

			var cmd = SDL_AcquireGPUCommandBuffer(device);
			var pass = SDL_BeginGPUCopyPass(cmd);

			var src = new SDL_GPUTextureTransferInfo {
				transfer_buffer = transfer,
				offset = 0, // todo: layers, mips
				pixels_per_row = 0, // tightly packed, defaults to width 
				rows_per_layer = 0, // tightly packed, defaults to height
			};

			var dst = new SDL_GPUTextureRegion {
				texture = DeviceTexture,
				w = TextureInfo.width,
				h = TextureInfo.height,
				d = TextureInfo.layer_count_or_depth,
				layer = 0, // todo: layers
				mip_level = 0, // todo: mips
				x = 0,
				y = 0,
				z = 0,
			};

			SDL_UploadToGPUTexture(pass, &src, &dst, false);

			SDL_EndGPUCopyPass(pass);
			SDL_SubmitGPUCommandBuffer(cmd);
			SDL_ReleaseGPUTransferBuffer(device, transfer);
		}

		if (DeviceSampler != null) {
			var info = SamplerInfo;
			DeviceSampler = SDL_CreateGPUSampler(device, &info);
		}
	}

	public override unsafe void Destroy() {
		if (DeviceSampler != null) {
			SDL_ReleaseGPUSampler(Manager.Scene.Renderer.DeviceHandle, DeviceSampler);
			DeviceSampler = null;
		}

		if (DeviceTexture != null) {
			SDL_ReleaseGPUTexture(Manager.Scene.Renderer.DeviceHandle, DeviceTexture);
			DeviceTexture = null;
		}
	}
}
