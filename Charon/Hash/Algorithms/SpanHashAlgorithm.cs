// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Security.Cryptography;

namespace Charon.Hash.Algorithms;

public abstract class SpanHashAlgorithm<T> : HashAlgorithm
	where T : unmanaged, INumber<T> {
	protected SpanHashAlgorithm() {
		unsafe {
			HashSizeValue = sizeof(T) * 8;
		}
	}

	public T Value { get; set; }

	public abstract void Reset();

	protected override void HashCore(ReadOnlySpan<byte> source) => throw new NotImplementedException();
	protected override void HashCore(byte[] array, int ibStart, int cbSize) => HashCore(new ReadOnlySpan<byte>(array, ibStart, cbSize));

	protected override byte[] HashFinal() {
		var tmp = GetValueFinal();
		Reset();
		return MemoryMarshal.AsBytes(new Span<T>(ref tmp)).ToArray();
	}

	public virtual void Update(ReadOnlySpan<byte> bytes) => HashCore(bytes);

	public virtual T ComputeHashValue(ReadOnlySpan<byte> bytes) {
		Update(bytes);
		return GetValueFinal();
	}

	public byte[] ComputeHash(ReadOnlySpan<byte> bytes) {
		Update(bytes);
		return HashFinal();
	}

	public abstract T GetValueFinal();
}
