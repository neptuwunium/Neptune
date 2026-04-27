// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;

namespace Ceres.Extensions;

public class NEPTUWUNIUMMaterialAttributes : Property, IExtension {
	[JsonPropertyName("textures")]
	public Dictionary<string, TextureInfo>? Textures { get; set; }

	[JsonPropertyName("scalars")]
	public Dictionary<string, double>? Scalars { get; set; }

	[JsonPropertyName("colors")]
	public Dictionary<string, List<double>>? Colors { get; set; }

	[JsonPropertyName("workflow")]
	public string? Workflow { get; set; }

	public static string ExtensionName => "NEPTUWUNIUM_material_attributes";
}
