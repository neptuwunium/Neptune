// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Runtime.CompilerServices;

namespace Pluto.IO.Binary;

public class ArrayPoolBinaryReader : BufferBinaryReader {
	public ArrayPoolBinaryReader(byte[] array, int length, bool leaveOpen = false) {
		LeaveOpen = leaveOpen;
		Length = length;
		Array = array;
		Rented = new RentedArray<byte>(array, length);
	}

	public ArrayPoolBinaryReader(IRentedArray<byte> array, bool leaveOpen = false) {
		LeaveOpen = leaveOpen;
		Length = array.Length;
		Rented = array;
	}

	public override int Position { get; set; }
	public override int Length { get; }
	protected byte[]? Array { get; set; }
	protected IRentedArray<byte> Rented { get; set; }

	public override void ReadBytes(Span<byte> span) {
		if (Array is { } array) {
			array.AsSpan(Position, Length).CopyTo(span);
		} else {
			Rented.Span.Slice(Position, span.Length).CopyTo(span);
		}
		
		Position += span.Length;
	}

	public bool LeaveOpen { get; }

	public override IRentedArray<T> ReadShared<T>(int length) where T : struct {
		var arr = new UnownedCovariantArray<T>(Rented, Position, length);
		Position += Unsafe.SizeOf<T>() * length;
		return arr;
	}

	public override IRentedArray<byte> ReadSharedBytes(int length) {
		var arr = new UnownedRentedArray<byte>(Rented, Position, length);
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
