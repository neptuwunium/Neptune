// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;

namespace Ceres;

public class NormalTextureInfo : TextureInfo {
	/// <summary>
	///     The scalar parameter applied to each normal vector of the texture. This value scales the normal vector in X
	///     and Y directions using the formula:
	///     <code>scaledNormal =  normalize((&lt;sampled normal texture value&gt; * 2.0 - 1.0) * vec3(&lt;normal scale&gt;, &lt;normal scale&gt;, 1.0))</code>
	/// </summary>
	[JsonPropertyName("scale")]
	public double? Scale { get; set; } = 1D;
}
