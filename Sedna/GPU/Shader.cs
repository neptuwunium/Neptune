// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using SDL;
using static SDL.SDL3;

namespace Sedna.GPU;

public class Shader : ManagedResource<ShaderResourceId> {
	internal Shader(ShaderResourceId value, ResourceManager manager) : base(value, manager) { }

	public string? Name { get; set; }
	public Dictionary<string, string?> Defines { get; set; } = [];
	public SDL_ShaderCross_ShaderStage Stage { get; set; }
	public string? IncludeDir { get; set; }
	public string EntryPoint { get; set; } = "shader_main";
	public string? ShaderCode { get; set; }

	public nint DeviceShaderCode { get; set; }
	public unsafe SDL_GPUShader* DeviceShader { get; set; }

	public override unsafe void Destroy() {
		if (DeviceShader != null) {
			SDL_ReleaseGPUShader(Manager.Scene.Renderer.DeviceHandle, DeviceShader);
			DeviceShader = null;
		}

		if (DeviceShaderCode != nint.Zero) {
			Marshal.FreeHGlobal(DeviceShaderCode);
		}
	}

	// todo List<BufferResourceId> ComputeBuffers -> SDL_GPU_BUFFERUSAGE_COMPUTE_STORAGE_READ | SDL_GPU_BUFFERUSAGE_COMPUTE_STORAGE_WRITE
}
