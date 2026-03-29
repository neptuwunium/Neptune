// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;

namespace Ceres;

/// <summary>A buffer points to binary geometry, animation, or skins.</summary>
public class Buffer : ChildOfRootProperty {
	/// <summary>
	///     The URI (or IRI) of the buffer. Relative paths are relative to the current glTF asset. Instead of referencing
	///     an external file, this field <b>MAY</b> contain a `data:`-URI.
	/// </summary>
	[JsonPropertyName("uri")]
	public string? Uri { get; set; }

	/// <summary>The length of the buffer in bytes.</summary>
	[JsonPropertyName("byteLength")]
	public long ByteLength { get; set; }
}
