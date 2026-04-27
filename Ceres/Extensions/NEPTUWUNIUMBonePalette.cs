// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;

namespace Ceres.Extensions;

public class NEPTUWUNIUMBonePalette : Property, IExtension {
	/// <summary>
	///     Index to palette accessor
	/// </summary>
	[JsonPropertyName("palette")]
	public required int Palette { get; set; }

	public static string ExtensionName => "NEPTUWUNIUM_bone_palette";
}
