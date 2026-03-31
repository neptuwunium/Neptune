// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

// ReSharper disable once CheckNamespace
namespace Pluto.SourceGen.ReverseEndiannessGenerator;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class EndianSwappableAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public sealed class DoNotSwapAttribute : Attribute;

/*

[EndianSwappable]
public partial struct PlatformIndependentStruct {
	public int Nya { get; set; }
	[DoNotSwap] public float Meow { get; set; }
}

*/
