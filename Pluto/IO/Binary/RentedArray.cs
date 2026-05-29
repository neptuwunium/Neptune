// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pluto.IO.Binary;

public interface IRentedArray<T> : IEnumerable<T>, IDisposable where T : struct {
	static IRentedArray<T> Empty { get; } = new RentedArray<T>();
	int Length { get; set; }
	Memory<T> Memory { get; }
	Span<T> Span { get; }
	T this[int index] { get; set; }
	IRentedArray<T> Clone();
}

public sealed class UnownedRentedArray<T> : IRentedArray<T> where T : struct {
	public UnownedRentedArray(IRentedArray<T> inner) {
		Inner = inner;
		Offset = 0;
		Length = inner.Length;
	}

	public UnownedRentedArray(IRentedArray<T> inner, int offset) {
		Inner = inner;
		Offset = offset;
		Length = inner.Length - offset;
	}

	public UnownedRentedArray(IRentedArray<T> inner, int offset, int length) {
		Inner = inner;
		Offset = offset;
		Length = length;
	}

	public IRentedArray<T> Inner { get; }
	public int Offset { get; }
	public int Length { get; set => field = value > Inner.Length ? throw new InvalidOperationException("cannot grow array") : value; }
	public Memory<T> Memory => Length == 0 ? Memory<T>.Empty : Inner.Memory.Slice(Offset, Length);
	public Span<T> Span => Length == 0 ? Span<T>.Empty : Inner.Span.Slice(Offset, Length);

	public T this[int index] {
		get => Inner[Offset + index];
		set => Inner[Offset + index] = value;
	}

	public IRentedArray<T> Clone() {
		var arr = new RentedArray<T>(Length);
		Span.CopyTo(arr.Span);
		return arr;
	}

	public void Dispose() {
		if (Length == 0) {
			return;
		}

		Length = 0;
	}

	public IEnumerator<T> GetEnumerator() {
		for (var i = 0; i < Length; ++i) {
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed class UnownedCovariantArray<T> : IRentedArray<T> where T : struct {
	public UnownedCovariantArray(IRentedArray<byte> inner) : this(inner, 0, inner.Length / Unsafe.SizeOf<T>()) { }
	public UnownedCovariantArray(IRentedArray<byte> inner, int byteOffset) : this(inner, byteOffset, (inner.Length - byteOffset) / Unsafe.SizeOf<T>()) { }

	public UnownedCovariantArray(IRentedArray<byte> inner, int byteOffset, int length) {
		Inner = inner;
		Offset = byteOffset;
		Length = length;
		Manager = new MemoryCastManager(Inner.Memory.Slice(Offset, ByteLength));
	}

	private MemoryCastManager Manager { get; }
	public IRentedArray<byte> Inner { get; }
	public int Offset { get; }
	public int ByteLength => Length * Unsafe.SizeOf<T>();
	public int Length { get; set => field = value * Unsafe.SizeOf<T>() > Inner.Length ? throw new InvalidOperationException("cannot grow array") : value; }
	public Memory<T> Memory => Length == 0 ? Memory<T>.Empty : Manager.Memory;
	public Span<T> Span => Length == 0 ? Span<T>.Empty : MemoryMarshal.Cast<byte, T>(Inner.Span.Slice(Offset, ByteLength));

	public IRentedArray<T> Clone() {
		var arr = new RentedArray<T>(Length);
		Span.CopyTo(arr.Span);
		return arr;
	}

	public T this[int index] {
		get => Span[index];
		set => Span[index] = value;
	}

	public void Dispose() {
		if (Length == 0) {
			return;
		}

		Length = 0;
	}

	public IEnumerator<T> GetEnumerator() {
		var slice = Memory;
		for (var i = 0; i < Length; ++i) {
			yield return slice.Span[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	private class MemoryCastManager(Memory<byte> from) : MemoryManager<T> {
		private Memory<byte> From { get; } = from;

		public override Span<T> GetSpan() => MemoryMarshal.Cast<byte, T>(From.Span);
		protected override void Dispose(bool disposing) { }
		public override MemoryHandle Pin(int elementIndex = 0) => throw new NotSupportedException();
		public override void Unpin() => throw new NotSupportedException();
	}
}

public sealed class RentedArray<T> : IRentedArray<T> where T : struct {
	public RentedArray(T[] array, int length) {
		Array = array;
		Length = length;
	}

	public RentedArray(int length) : this(length == 0 ? [] : ArrayPool<T>.Shared.Rent(length), length) { }

	public RentedArray() {
		Array = [];
		Length = 0;
	}

	~RentedArray() => Dispose(false);

	public T[] Array { get; private set; }
	public ArraySegment<T> Segment => Length == 0 ? ArraySegment<T>.Empty : new ArraySegment<T>(Array, 0, Length);
	public static RentedArray<T> Empty { get; } = new();
	public int Length { get; set => field = value > Array.Length ? throw new InvalidOperationException("cannot grow array") : value; }
	public Memory<T> Memory => Length == 0 ? Memory<T>.Empty : Array.AsMemory(0, Length);
	public Span<T> Span => Length == 0 ? Span<T>.Empty : Array.AsSpan(0, Length);

	public IRentedArray<T> Clone() {
		var arr = new RentedArray<T>(Length);
		Span.CopyTo(arr.Span);
		return arr;
	}

	public T this[int index] {
		get => Array[index];
		set => Array[index] = value;
	}

	public void Dispose(bool disposing) {
		if (Length == 0) {
			return;
		}

		ArrayPool<T>.Shared.Return(Array);
		Array = [];
		Length = 0;
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public IEnumerator<T> GetEnumerator() {
		for (var i = 0; i < Length; ++i) {
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public static RentedArray<T> FromFile(string path) => FromStream(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), false);

	public static RentedArray<T> FromStream(Stream stream, bool leaveOpen) {
		try {
			var buffer = new RentedArray<T>((int) (stream.Length - stream.Position) / Unsafe.SizeOf<T>());
			stream.ReadExactly(MemoryMarshal.AsBytes(buffer.Span));
			return buffer;
		} finally {
			if (!leaveOpen) {
				stream.Close();
				stream.Dispose();
			}
		}
	}

	public int BinarySearch(int index, int count, T item, IComparer<T>? comparer) {
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		ArgumentOutOfRangeException.ThrowIfNegative(count);
		ArgumentOutOfRangeException.ThrowIfLessThan(Length - index, count);

		comparer ??= Comparer<T>.Default;

		var lo = index;
		var hi = index + count - 1;

		while (lo <= hi) {
			var i = lo + ((hi - lo) >> 1);
			var order = comparer.Compare(Array[i], item);

			switch (order) {
				case 0:
					return i;
				case < 0:
					lo = i + 1;
					break;
				default:
					hi = i - 1;
					break;
			}
		}

		return ~lo;
	}

	public int BinarySearch(T item) => BinarySearch(0, Length, item, null);
	public int BinarySearch(T item, IComparer<T>? comparer) => BinarySearch(0, Length, item, comparer);
}
