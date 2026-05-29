// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Compression.Zip;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct ZipFileHeader {
	public const uint MAGIC = 0x04034B50;

	public uint Magic { get; init; }
	public ushort Version { get; init; }
	public ushort Flags { get; init; }
	public ushort Compression { get; init; }
	public ushort ModTime { get; init; }
	public ushort ModDate { get; init; }
	public uint Checksum { get; init; }
	public int DiskSize { get; init; }
	public int MemorySize { get; init; }
	public ushort FileNameLength { get; init; }
	public ushort ExtraFieldLength { get; init; }
}
