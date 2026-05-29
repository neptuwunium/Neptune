// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Compression.Zip;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct ZipEndOfCentralDirectory {
	public const uint MAGIC = 0x06054B50;

	public uint Magic { get; init; }
	public ushort DiskNumber { get; init; }
	public ushort DirectoryDiskNumber { get; init; }
	public ushort NumberOfDirectoryRecords { get; init; }
	public ushort TotalNumberOfDirectoryRecords { get; init; }
	public uint DirectorySize { get; init; }
	public uint DirectoryOffset { get; init; }
	public ushort CommentLength { get; init; }

	public bool IsZip64 => DirectoryOffset == uint.MaxValue;
}
