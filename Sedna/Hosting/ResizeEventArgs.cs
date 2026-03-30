// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Pluto.Maths;

namespace Sedna.Hosting;

public class ResizeEventArgs : EventArgs {
	public ResizeEventArgs(int width, int height) => Size = new Point<int>(width, height);

	public ResizeEventArgs(Point<int> size) => Size = size;

	public Point<int> Size { get; }
}
