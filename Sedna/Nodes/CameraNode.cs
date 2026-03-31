// SPDX-FileCopyrightText: 2026 Neptuwunium <ada@chronovore.dev>
//
// SPDX-License-Identifier: EUPL-1.2

using Silk.NET.Maths;

namespace Sedna.Nodes;

public class CameraNode : Node {
	public float FieldOfView { get; set; }
	public float NearClip { get; set; }
	public float FarClip { get; set; }
	public Matrix4X4<float> ProjectionMatrix { get; set; }
}
