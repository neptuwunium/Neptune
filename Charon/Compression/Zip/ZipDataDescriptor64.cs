// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Compression.Zip;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct ZipDataDescriptor64 {
	public uint Checksum { get; init; }
	public ulong DiskSize { get; init; }
	public ulong MemorySize { get; init; }
}
