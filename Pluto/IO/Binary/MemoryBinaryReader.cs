// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Pluto.IO.Binary;

public class MemoryBinaryReader : BufferBinaryReader {
	public MemoryBinaryReader(Memory<byte> memory) => Memory = memory;

	public Memory<byte> Memory { get; }
	public override long Position { get; set; }
	public override long Length {
		get => Memory.Length;
		protected set => throw new NotSupportedException();
	}

	public override void ReadBytes(Span<byte> span) {
		Memory.Span.Slice(checked((int) Position), span.Length).CopyTo(span);
		Position += span.Length;
	}

	protected override void Dispose(bool disposing) { }
}
