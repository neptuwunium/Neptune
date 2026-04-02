// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

// ReSharper disable once CheckNamespace

namespace Pluto.SourceGen.MagicGenerator;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class GenerateMagicAttribute(int size = 4) : Attribute {
	public int Size { get; set; } = size;
}

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public sealed class MagicAttribute(string magic, string? name = null, bool littleEndian = true) : Attribute {
	public string Magic { get; } = magic;
	public string Name { get; } = name ?? magic;
	public bool LittleEndian { get; } = littleEndian;
}

/*

[GenerateMagic(8)]
[Magic("IDX ", "Index")]
[Magic("DATA", "Data", false)]
public partial record struct FileMagic;

*/
