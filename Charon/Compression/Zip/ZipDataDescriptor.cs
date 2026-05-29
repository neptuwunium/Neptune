// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Compression.Zip;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct ZipDataDescriptor {
	public uint Checksum { get; init; }
	public uint DiskSize { get; init; }
	public uint MemorySize { get; init; }
}
