// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics.CodeAnalysis;

namespace Pluto;

public static class Singleton<T> where T : class, new() {
	[field: AllowNull, MaybeNull]
	public static T Instance {
		get => field ??= new T();
		set;
	}
}
