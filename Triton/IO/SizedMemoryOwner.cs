// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Runtime.InteropServices;

namespace Triton.IO;

public sealed class SizedMemoryOwner<T> : IMemoryOwner<T> where T : struct {
	public SizedMemoryOwner(FileInfo info) : this((int) info.Length) {
		using var stream = new FileStream(info.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		stream.ReadExactly(MemoryMarshal.AsBytes(Memory.Span));
	}

	public SizedMemoryOwner(int length) {
		UnderlyingOwner = MemoryPool<T>.Shared.Rent(length);
		Length = length;
	}

	public IMemoryOwner<T>? UnderlyingOwner { get; private set; }
	public int Offset { get; set; }
	public int Length { get; }

	public void Dispose() {
		Free();
		GC.SuppressFinalize(this);
	}

	public Memory<T> Memory => UnderlyingOwner!.Memory.Slice(Offset, Length - Offset);

	~SizedMemoryOwner() => Free();

	private void Free() {
		UnderlyingOwner?.Dispose();
		UnderlyingOwner = null;
	}
}
