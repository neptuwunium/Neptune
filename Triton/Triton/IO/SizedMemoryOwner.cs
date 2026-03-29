// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2 OR LGPL-3.0-or-later
// You may choose either license when using or modifying this code.

using System.Buffers;
using System.Runtime.InteropServices;

namespace Triton.IO;

public sealed class SizedMemoryOwner<T> : IMemoryOwner<T> where T : struct {
	public SizedMemoryOwner(FileInfo info) : this((int) info.Length) {
		if (Length == 0) {
			return;
		}

		using var stream = new FileStream(info.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		stream.ReadExactly(MemoryMarshal.AsBytes(Memory.Span));
	}

	public SizedMemoryOwner(int length) {
		Length = length;
		if (length > 0) {
			UnderlyingOwner = MemoryPool<T>.Shared.Rent(length);
		}
	}

	public SizedMemoryOwner(IMemoryOwner<T> owner, int offset, int length, bool shouldDispose = true) {
		ShouldDispose = shouldDispose;
		UnderlyingOwner = owner;
		Offset = offset;
		Length = length;
	}

	public static SizedMemoryOwner<T> Empty { get; } = new(0);

	private bool ShouldDispose { get; }
	public IMemoryOwner<T>? UnderlyingOwner { get; private set; }
	public int Offset { get; set; }
	public int Length { get; }

	public void Dispose() {
		Free();
		GC.SuppressFinalize(this);
	}

	public Memory<T> Memory => UnderlyingOwner!.Memory.Slice(Offset, Length);

	~SizedMemoryOwner() => Free();

	private void Free() {
		if (ShouldDispose) {
			UnderlyingOwner?.Dispose();
		}

		UnderlyingOwner = null;
	}

	public IMemoryOwner<T> Clone() {
		var owner = new SizedMemoryOwner<T>(Length);
		owner.Offset = Offset;
		Memory.CopyTo(owner.Memory);
		return owner;
	}
}
