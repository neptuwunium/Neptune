// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;

namespace Ceres.Extensions;

public class KHRTextureTransform : Property, IExtension {
	public static string ExtensionName => "KHR_texture_transform";

	[JsonPropertyName("offset")]
	public List<double>? Offset { get; set; }

	[JsonPropertyName("rotation")]
	public double? Rotation { get; set; }

	[JsonPropertyName("scale")]
	public List<double>? Scale { get; set; }

	[JsonPropertyName("texCoord")]
	public int? TexCoord { get; set; }
}
