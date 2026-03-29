// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json.Serialization;

namespace Ceres;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnimationInterpolation {
	LINEAR,
	STEP,
	CUBICSPLINE,
}
