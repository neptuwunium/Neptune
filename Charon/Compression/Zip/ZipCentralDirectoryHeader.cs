// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Compression.Zip;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct ZipCentralDirectoryHeader {
	public const uint MAGIC = 0x02014B50;

	public uint Magic { get; init; }
	public ushort VersionMadeBy { get; init; }
	public ushort VersionNeeded { get; init; }
	public ushort Flags { get; init; }
	public ushort Compression { get; init; }
	public ushort ModTime { get; init; }
	public ushort ModDate { get; init; }
	public uint Checksum { get; init; }
	public uint DiskSize { get; init; }
	public uint MemorySize { get; init; }
	public ushort FileNameLength { get; init; }
	public ushort ExtraFieldLength { get; init; }
	public ushort CommentLength { get; init; }
	public ushort DiskNumber { get; init; }
	public ushort InternalFileAttributes { get; init; }
	public uint ExternalFileAttributes { get; init; }
	public uint Offset { get; init; }

	public DateTimeOffset ModDateTime {
		get {
			if (ModTime == 0 && ModDate == 0) {
				return DateTimeOffset.UnixEpoch;
			}

			var sc = (ModTime & 0x1F) << 1;
			var mn = (ModTime & 0x7E0) >> 5;
			var hr = (ModTime & 0xF800) >> 11;
			var dy = ModDate & 0x1F;
			var mo = (ModDate & 0x1E0) >> 5;
			var yr = (ModDate & 0xFE00) >> 9;
			return new DateTimeOffset(yr + 1980, mo, dy, hr, mn, sc, TimeSpan.Zero);
		}
	}

	public bool IsSupported => (ZipCompression) Compression is
		ZipCompression.Store or
		ZipCompression.Deflate or
		ZipCompression.Deflate64;
}
