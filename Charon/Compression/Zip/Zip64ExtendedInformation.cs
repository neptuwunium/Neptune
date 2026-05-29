// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Compression.Zip;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct Zip64ExtendedInformation {
	public long MemorySize { get; init; }
	public long DiskSize { get; init; }
	public long Offset { get; init; }
	public uint DiskNumber { get; init; }
}
