// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

// ReSharper disable once CheckNamespace

namespace Pluto.SourceGen.TransparentStructGenerator;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class TransparentStructAttribute<T>(string label = "Value") : Attribute {
	public Type Type { get; } = typeof(T);
	public string Label { get; } = label;
}

/*

[TransparentStruct<ulong>] public partial struct ResourceId;

*/
