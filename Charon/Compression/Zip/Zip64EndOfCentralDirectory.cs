// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Compression.Zip;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct Zip64EndOfCentralDirectory {
	public const uint MAGIC = 0x06064B50;

	public uint Magic { get; init; }
	public ulong Size { get; init; }
	public ushort VersionBy { get; init; }
	public ushort VersionExtract { get; init; }
	public uint DiskNumber { get; init; }
	public uint DirectoryDiskNumber { get; init; }
	public long NumberOfDirectoryRecords { get; init; }
	public long TotalNumberOfDirectoryRecords { get; init; }
	public long DirectorySize { get; init; }
	public long DirectoryOffset { get; init; }
}
