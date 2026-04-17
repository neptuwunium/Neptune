// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
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

	public MemoryHandle TextureHandle { get; set; }
	public unsafe SDL_GPUSampler* DeviceSampler { get; set; }
	public unsafe SDL_GPUTexture* DeviceTexture { get; set; }

	public override unsafe void Destroy() {
		if (TextureHandle.Pointer != null) {
			TextureHandle.Dispose();
			TextureHandle = default;
		}
		
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
