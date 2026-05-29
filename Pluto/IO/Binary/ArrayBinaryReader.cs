// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Pluto.IO.Binary;

public class ArrayBinaryReader : BufferBinaryReader {
	public ArrayBinaryReader(byte[] array) => Array = array;

	public byte[] Array { get; }
	public override long Position { get; set; }
	public override long Length {
		get => Array.Length;
		protected set => throw new NotSupportedException();
	}

	public override void ReadBytes(Span<byte> span) {
		Array.AsSpan(checked((int) Position), span.Length).CopyTo(span);
		Position += span.Length;
	}

	protected override void Dispose(bool disposing) { }
}
