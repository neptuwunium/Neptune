// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Ceres.Extensions;

public class KHRLightsPunctual : Property, IExtension {
	public static string ExtensionName => "KHR_lights_punctual";

	public List<KHRLight>? Lights { get; set; }
	public int? Light { get; set; }

	public (KHRLight Light, int Id) CreateLight(string name) {
		Lights ??= [];
		var id = Lights.Count;
		var light = new KHRLight {
			Name = name,
		};
		Lights.Add(light);
		return (light, id);
	}
}
