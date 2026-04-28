// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Sedna.Nodes;
using Sedna.Render;

namespace Sedna;

public class SednaScene {
	public SednaScene(RenderLoop renderer) {
		Renderer = renderer;
		Resources = new ResourceManager(this);
	}

	public Node Root { get; } = new();
	public ResourceManager Resources { get; }
	public RenderLoop Renderer { get; }
	public PipelineCache PipelineCache { get; } = new();
}
