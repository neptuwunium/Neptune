// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;

namespace Pluto.IO.Binary;

public class ArrayPoolBinaryReader : BufferBinaryReader {
	public ArrayPoolBinaryReader(byte[] array, int length, bool leaveOpen = false) {
		LeaveOpen = leaveOpen;
		Length = length;
		Rented = new RentedArray<byte>(array, length);
		SetLength(length);
	}

	public ArrayPoolBinaryReader(IRentedArray<byte> array, bool leaveOpen = false) {
		LeaveOpen = leaveOpen;
		Rented = array;
		SetLength(array.Length);
	}

	public override long Position { get; set; }
	public override long Length { get; protected set; }
	protected byte[]? Array { get; set; }
	protected IRentedArray<byte> Rented { get; set; }

	public bool LeaveOpen { get; }

	protected void SetLength(int length) => Length = length;

	public override void ReadBytes(Span<byte> span) {
		if (Array is { } array) {
			array.AsSpan(checked((int) Position), checked((int) Length)).CopyTo(span);
		} else {
			Rented.Span.Slice(checked((int) Position), span.Length).CopyTo(span);
		}

		Position += span.Length;
	}

	public override IRentedArray<T> ReadShared<T>(int length) where T : struct {
		var arr = new UnownedCovariantArray<T>(Rented, checked((int) Position), length);
		Position += Unsafe.SizeOf<T>() * length;
		return arr;
	}

	public override IRentedArray<byte> ReadSharedBytes(int length) {
		var arr = new UnownedRentedArray<byte>(Rented, checked((int) Position), length);
		Position += length;
		return arr;
	}

	protected override void Dispose(bool disposing) {
		if (LeaveOpen) {
			return;
		}

		Rented.Dispose();
	}
}
