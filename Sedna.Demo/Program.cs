// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Sedna.Hosting.SDL;
using Sedna.Render;

namespace Sedna.Demo;

internal static class Program {
	private static void Main(string[] args) {
		using var host = new SDLHost("Sedna", 800, 600);
		using var renderer = new RenderLoop();
		renderer.Attach(host);

		while (renderer.CanRender) {
			renderer.Render();
		}
	}
}
