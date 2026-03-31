// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Triton.Pixel.Formats;

namespace Sedna.Nodes;

public abstract class LightNode : Node {
	public ColorRGBA<float> Color { get; set; }
	public float Intensity { get; set; }
}
