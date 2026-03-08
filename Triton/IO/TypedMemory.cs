// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2 OR LGPL-3.0-or-later
// You may choose either license when using or modifying this code.

using System.Buffers;
using System.Runtime.CompilerServices;

namespace Triton.IO;

public sealed class TypedMemory<T> : IMemoryOwner<T> where T : struct {
	public TypedMemory(IMemoryOwner<byte> underlyingOwner, int offset) : this(underlyingOwner, offset, (underlyingOwner.Memory.Length - offset) / Unsafe.SizeOf<T>()) { }

	public TypedMemory(IMemoryOwner<byte> underlyingOwner, int realOffset, int length) {
		Manager = new MemoryTypeManager<T, byte>(underlyingOwner.Memory.Slice(realOffset, length * Unsafe.SizeOf<T>()));
		RealOffset = realOffset;
		Length = length;
	}

	public MemoryTypeManager<T, byte>? Manager { get; private set; }
	public int Offset { get; set; }
	public int RealOffset { get; set; }
	public int Length { get; set; }

	public void Dispose() {
		Free();
		GC.SuppressFinalize(this);
	}

	public Memory<T> Memory => Length <= 0 ? Memory<T>.Empty : Manager!.Memory;

	~TypedMemory() => Free();

	private void Free() {
		(Manager as IDisposable)?.Dispose();
		Manager = null;
	}
}
