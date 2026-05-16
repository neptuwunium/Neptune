// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.IO.MemoryMappedFiles;

namespace Pluto.IO.Binary;

public class MemoryMapBinaryReader : BufferBinaryReader {
	public MemoryMapBinaryReader(MemoryMappedFile file, long offset = 0, long length = 0, bool leaveOpen = false) {
		File = file;
		LeaveOpen = leaveOpen;
		Accessor = file.CreateViewAccessor(offset, length, MemoryMappedFileAccess.Read);
		Length = Accessor.Capacity > int.MaxValue ? int.MaxValue : (int) Accessor.Capacity;
	}


	public MemoryMapBinaryReader(string path, long offset = 0, long length = 0, MemoryMappedFileAccess access = MemoryMappedFileAccess.Read, bool leaveOpen = false) : this(MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, access), offset, length, leaveOpen) { }

	public MemoryMapBinaryReader(FileInfo fileInfo, long offset = 0, long length = 0, MemoryMappedFileAccess access = MemoryMappedFileAccess.Read, bool leaveOpen = false) : this(fileInfo.FullName, offset, length, access, leaveOpen) { }

	public MemoryMappedViewAccessor Accessor { get; set; }
	public MemoryMappedFile File { get; }
	public bool LeaveOpen { get; }

	public override int Position { get; set; }
	public override int Length { get; }

	public override void ReadBytes(Span<byte> span) {
		Accessor.SafeMemoryMappedViewHandle.ReadSpan((ulong) (Position + Accessor.PointerOffset), span);
		Position += span.Length;
	}

	protected override void Dispose(bool disposing) {
		Accessor.Dispose();

		if (LeaveOpen) {
			return;
		}

		File.Dispose();
	}
}
