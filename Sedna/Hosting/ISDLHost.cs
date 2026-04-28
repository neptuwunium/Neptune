// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using SDL;

namespace Sedna.Hosting;

public interface ISDLHost : IDisposable {
	unsafe SDL_Window* WindowHandle { get; }

	int Width { get; }
	int Height { get; }

	event EventHandler<ResizeEventArgs>? OnResize;
	event EventHandler? OnClose;

	void PollEvents();
}
