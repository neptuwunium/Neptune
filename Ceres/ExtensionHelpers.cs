// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;

namespace Ceres;

public static class ExtensionHelpers {
	public static List<double> ToGLTF(this Vector2 vector) => [vector.X, vector.Y];

	public static List<double> ToGLTF(this Vector3 vector) => [vector.X, vector.Y, vector.Z];

	public static List<double> ToGLTF(this Vector4 vector) => [vector.X, vector.Y, vector.Z, vector.W];

	public static List<double> ToGLTF(this Quaternion quaternion) => [quaternion.X, quaternion.Y, quaternion.Z, quaternion.W];
}
