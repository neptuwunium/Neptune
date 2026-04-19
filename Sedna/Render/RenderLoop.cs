// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Pluto.IO.Binary;
using SDL;
using Sedna.Hosting;
using static SDL.SDL3;

namespace Sedna.Render;

public class RenderLoop : IDisposable {
	public ISDLHost? Host { get; private set; }
	public unsafe SDL_GPUDevice* DeviceHandle { get; private set; }
	public unsafe SDL_Window* WindowHandle => Host == null ? null : Host.WindowHandle;

	[MemberNotNullWhen(true, nameof(DeviceHandle))]
	[MemberNotNullWhen(true, nameof(WindowHandle))]
	[MemberNotNullWhen(true, nameof(Host))]
	public unsafe bool CanRender => DeviceHandle != null && WindowHandle != null && Host != null;

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public unsafe void Attach(ISDLHost host) {
		if (Host != null) return;

		Host = host;

		var format = OperatingSystem.IsMacOS() || OperatingSystem.IsIOS() ? SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL : SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV;
		DeviceHandle = SDL_CreateGPUDevice(format, Debugger.IsAttached, default(byte*));

		if (!SDL_ClaimWindowForGPUDevice(DeviceHandle, WindowHandle)) {
			Dispose();
			return;
		}

		SDL_SetGPUSwapchainParameters(DeviceHandle, WindowHandle, SDL_GPUSwapchainComposition.SDL_GPU_SWAPCHAINCOMPOSITION_SDR, SDL_GPUPresentMode.SDL_GPU_PRESENTMODE_VSYNC);

		Host.OnResize += OnHostResized;
		Host.OnClose += OnHostClosed;
	}

	public virtual unsafe void Render() {
		if (!CanRender) return;

		if (Host.Width <= 1 || Host.Height <= 1) return;

		Host.PollEvents();

		if (SDL_GetWindowFlags(WindowHandle).HasFlag(SDL_WindowFlags.SDL_WINDOW_HIDDEN)) return;

		var cmdBuffer = SDL_AcquireGPUCommandBuffer(DeviceHandle);
		if (cmdBuffer == null) {
			Debug.WriteLine($"[Sedna] Command Buffer Acquire Failed: {SDL_GetError()}");
			Dispose();
			return;
		}

		SDL_GPUTexture* swapchainTexture;
		uint w, h;

		if (!SDL_WaitAndAcquireGPUSwapchainTexture(cmdBuffer, WindowHandle, &swapchainTexture, &w, &h)) {
			Debug.WriteLine($"[Sedna] GPU Swapchain Texture Acquire Failed: {SDL_GetError()}");
			SDL_SubmitGPUCommandBuffer(cmdBuffer);
			return;
		}

		Console.WriteLine($"Swapchain Texture Size: {w}x{h}");
		var colorTarget = new SDL_GPUColorTargetInfo {
			texture = swapchainTexture,
			clear_color = new SDL_FColor { r = 1.0f, g = 0.07f, b = 0.57f, a = 1.0f },
			load_op = SDL_GPULoadOp.SDL_GPU_LOADOP_CLEAR,
			store_op = SDL_GPUStoreOp.SDL_GPU_STOREOP_STORE,
		};

		var renderPass = SDL_BeginGPURenderPass(cmdBuffer, &colorTarget, 1, null);
		SDL_EndGPURenderPass(renderPass);
		SDL_SubmitGPUCommandBuffer(cmdBuffer);
	}

	private void OnHostClosed(object? sender, EventArgs e) => ReleaseUnmanagedResources();

	private unsafe void OnHostResized(object? sender, ResizeEventArgs e) {
		if (!CanRender) return;

		SDL_SetGPUSwapchainParameters(
			DeviceHandle,
			WindowHandle,
			SDL_GPUSwapchainComposition.SDL_GPU_SWAPCHAINCOMPOSITION_SDR,
			SDL_GPUPresentMode.SDL_GPU_PRESENTMODE_VSYNC
		);
	}
	
	public unsafe (nint Buffer, nint Transfer) UploadBuffer(IRentedArray<byte>? data, SDL_GPUBufferUsageFlags usage, SDL_GPUCopyPass* pass) {
		if (data == null) {
			return (nint.Zero, nint.Zero);
		}

		var bufferSize = (uint) data.Length;
		var bufferInfo = new SDL_GPUBufferCreateInfo {
			usage = usage,
			size = bufferSize,
			props = 0,
		};

		// todo: SDL_PROP_GPU_BUFFER_CREATE_NAME_STRING
		var buffer = SDL_CreateGPUBuffer(DeviceHandle, &bufferInfo);
		if (buffer == null) {
			// todo logging
			return (nint.Zero, nint.Zero);
		}

		var transferInfo = new SDL_GPUTransferBufferCreateInfo {
			usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
			size = bufferSize,
		};

		// todo: SDL_PROP_GPU_TRANSFERBUFFER_CREATE_NAME_STRING
		var transferBuffer = SDL_CreateGPUTransferBuffer(DeviceHandle, &transferInfo);
		if (transferBuffer == null) {
			// todo logging
			SDL_ReleaseGPUBuffer(DeviceHandle, buffer);
			return (nint.Zero, nint.Zero);
		}

		var map = SDL_MapGPUTransferBuffer(DeviceHandle, transferBuffer, false);
		if (map == nint.Zero) {
			// todo logging
			SDL_ReleaseGPUBuffer(DeviceHandle, buffer);
			SDL_ReleaseGPUTransferBuffer(DeviceHandle, transferBuffer);
			return (nint.Zero, nint.Zero);
		}

		using (var pinned = data.Memory.Pin()) {
			Buffer.MemoryCopy(pinned.Pointer, (void*) map, bufferSize, bufferSize);
		}

		SDL_UnmapGPUTransferBuffer(DeviceHandle, transferBuffer);

		var src = new SDL_GPUTransferBufferLocation { transfer_buffer = transferBuffer, offset = 0 };
		var dst = new SDL_GPUBufferRegion { buffer = buffer, offset = 0, size = bufferSize };

		SDL_UploadToGPUBuffer(pass, &src, &dst, false);

		return ((nint) buffer, (nint) transferBuffer);
	}

	private unsafe void ReleaseUnmanagedResources() {
		if (DeviceHandle != null) {
			SDL_WaitForGPUIdle(DeviceHandle);
			SDL_ReleaseWindowFromGPUDevice(DeviceHandle, WindowHandle);
			SDL_DestroyGPUDevice(DeviceHandle);
			DeviceHandle = null;
		}
	}

	protected virtual void Dispose(bool disposing) {
		ReleaseUnmanagedResources();

		if (disposing && Host is { } host) {
			Host = null;
			host.Dispose();
		}
	}

	~RenderLoop() => Dispose(false);
}
