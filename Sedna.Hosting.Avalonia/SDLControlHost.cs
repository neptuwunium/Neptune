// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using SDL;
using Sedna.Render;
using static SDL.SDL3;

namespace Sedna.Hosting.Avalonia;

public class SDLControlHost : NativeControlHost, ISDLHost {
	public RenderLoop RenderLoop { get; } = new();
	public unsafe SDL_Window* WindowHandle { get; private set; }
	int ISDLHost.Width => (int) Bounds.Width;
	int ISDLHost.Height => (int) Bounds.Height;
	public event EventHandler<ResizeEventArgs>? OnResize;
	public event EventHandler? OnClose;

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public unsafe void PollEvents() {
		if (WindowHandle == null) {
			return;
		}

		SDL_Event evt = new();
		while (SDL_PollEvent(&evt)) { }
	}

	protected override unsafe IPlatformHandle CreateNativeControlCore(IPlatformHandle parent) {
		var handle = base.CreateNativeControlCore(parent);
		var windowHandle = handle.Handle;
		var props = SDL_CreateProperties();

		try {
			if (OperatingSystem.IsWindows()) {
				SDL_SetPointerProperty(props, SDL_PROP_WINDOW_CREATE_WIN32_HWND_POINTER, windowHandle);
			} else if (OperatingSystem.IsMacOS()) {
				SDL_SetPointerProperty(props, SDL_PROP_WINDOW_CREATE_COCOA_WINDOW_POINTER, windowHandle);
			} else if (OperatingSystem.IsLinux()) {
				// todo: change me once avalonia gets wayland
				// note: this will probably be super cursed on wayland since the whole nesting thing is ass. 
				SDL_SetHint(SDL_HINT_VIDEO_DRIVER, "x11");
				SDL_SetEnvironmentVariable(SDL_GetEnvironment(), SDL_HINT_VIDEO_DRIVER, "x11", true);
				SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_X11_WINDOW_NUMBER, windowHandle);
			}

			SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_EXTERNAL_GRAPHICS_CONTEXT_BOOLEAN, true);
			SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_BORDERLESS_BOOLEAN, true);
			var isDarwin = OperatingSystem.IsMacOS() || OperatingSystem.IsIOS();
			SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_METAL_BOOLEAN, isDarwin);
			SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_VULKAN_BOOLEAN, !isDarwin);
			SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_WIDTH_NUMBER, (int) Bounds.Width);
			SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_HEIGHT_NUMBER, (int) Bounds.Height);

			WindowHandle = SDL_CreateWindowWithProperties(props);
		} finally {
			SDL_DestroyProperties(props);
		}

		if (WindowHandle == null) {
			Debug.WriteLine($"[Sedna] Window Acquire Failed: {SDL_GetError()}");
			return handle;
		}

		SDL_ShowWindow(WindowHandle);
		RenderLoop.Attach(this);
		ElementComposition.GetElementVisual(this)?.Compositor.RequestCompositionUpdate(OnCompositionUpdate);
		return handle;
	}

	private void OnCompositionUpdate() {
		if (RenderLoop is not { CanRender: true }) {
			return;
		}

		RenderLoop.Render();
		ElementComposition.GetElementVisual(this)?.Compositor.RequestCompositionUpdate(OnCompositionUpdate);
	}

	protected override void DestroyNativeControlCore(IPlatformHandle control) {
		OnClose?.Invoke(this, EventArgs.Empty);
		ReleaseUnmanagedResources();
		RenderLoop.Dispose();
	}

	protected override void OnSizeChanged(SizeChangedEventArgs e) {
		base.OnSizeChanged(e);

		var pixelWidth = (int) Bounds.Width;
		var pixelHeight = (int) Bounds.Height;

		OnResize?.Invoke(this, new ResizeEventArgs(pixelWidth, pixelHeight));
	}

	private unsafe void ReleaseUnmanagedResources() {
		if (WindowHandle != null) {
			SDL_DestroyWindow(WindowHandle);
			WindowHandle = null;
		}
	}

	protected virtual void Dispose(bool disposing) {
		ReleaseUnmanagedResources();

		if (disposing) {
			RenderLoop.Dispose();
		}
	}

	~SDLControlHost() => Dispose(false);
}
