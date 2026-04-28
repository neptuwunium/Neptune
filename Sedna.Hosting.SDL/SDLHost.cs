// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using SDL;
using static SDL.SDL3;

namespace Sedna.Hosting.SDL;

public class SDLHost : ISDLHost {
	public unsafe SDLHost(string title, int width, int height, SDL_WindowFlags flags = SDL_WindowFlags.SDL_WINDOW_RESIZABLE) {
		Width = width;
		Height = height;

		var props = SDL_CreateProperties();
		try {
			SDL_SetStringProperty(props, SDL_PROP_WINDOW_CREATE_TITLE_STRING, title);
			SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_WIDTH_NUMBER, width);
			SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_HEIGHT_NUMBER, height);
			SDL_SetNumberProperty(props, SDL_PROP_WINDOW_CREATE_FLAGS_NUMBER, (uint) flags);
			SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_HIGH_PIXEL_DENSITY_BOOLEAN, true);
			var isDarwin = OperatingSystem.IsMacOS() || OperatingSystem.IsIOS();
			SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_METAL_BOOLEAN, isDarwin);
			SDL_SetBooleanProperty(props, SDL_PROP_WINDOW_CREATE_VULKAN_BOOLEAN, !isDarwin);

			WindowHandle = SDL_CreateWindowWithProperties(props);
		} finally {
			SDL_DestroyProperties(props);
		}

		if (WindowHandle == null) {
			Debug.WriteLine($"[Sedna] Window Acquire Failed: {SDL_GetError()}");
		}
	}

	public unsafe SDL_Window* WindowHandle { get; private set; }
	public int Width { get; private set; }
	public int Height { get; private set; }
	public event EventHandler<ResizeEventArgs>? OnResize;
	public event EventHandler? OnClose;

	public unsafe void PollEvents() {
		if (WindowHandle == null) {
			return;
		}

		SDL_Event evt = new();
		while (SDL_PollEvent(&evt)) {
			switch (evt.Type) {
				case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
					Width = evt.window.data1;
					Height = evt.window.data2;
					OnResize?.Invoke(this, new ResizeEventArgs(Width, Height));
					break;
				case SDL_EventType.SDL_EVENT_QUIT:
					OnClose?.Invoke(this, EventArgs.Empty);
					break;
			}

			if (WindowHandle == null) {
				return;
			}
		}
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private unsafe void ReleaseUnmanagedResources() {
		if (WindowHandle != null) {
			SDL_DestroyWindow(WindowHandle);
			WindowHandle = null;
		}
	}

	protected virtual void Dispose(bool disposing) => ReleaseUnmanagedResources();

	~SDLHost() => Dispose(false);
}
