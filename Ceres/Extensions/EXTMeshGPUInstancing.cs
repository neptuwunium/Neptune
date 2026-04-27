// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;

namespace Ceres.Extensions;

public class EXTMeshGPUInstancing : Property, IExtension {
	[JsonPropertyName("attributes")]
	public Dictionary<string, int> Attributes { get; set; } = new();

	public static string ExtensionName => "EXT_mesh_gpu_instancing";
}
