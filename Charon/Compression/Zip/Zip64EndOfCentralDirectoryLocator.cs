// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Compression.Zip;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct Zip64EndOfCentralDirectoryLocator {
	public const uint MAGIC = 0x07064B50;

	public uint Magic { get; init; }
	public uint DiskNumber { get; init; }
	public long Offset { get; init; }
	public uint TotalNumberOfDisks { get; init; }
}
