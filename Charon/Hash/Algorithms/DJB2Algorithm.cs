// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;

namespace Charon.Hash.Algorithms;

// https://theartincode.stanis.me/008-djb2/
// ReSharper disable UnusedMethodReturnValue.Global
public sealed class DJB2Algorithm<T> : SpanHashAlgorithm<T>
	where T : unmanaged, INumber<T>, IBinaryInteger<T> {
	private readonly T Basis;

	public DJB2Algorithm(T basis) {
		Basis = basis;
		Reset(Basis);
	}

	public T HashNext(T value) {
		Value = (Value << 5) + Value + value;
		return Value;
	}

	public void HashCore<TOuter>(ReadOnlySpan<TOuter> source) where TOuter : INumberBase<TOuter>? {
		foreach (var value in source) {
			HashNext(T.CreateSaturating(value));
		}
	}

	protected override void HashCore(ReadOnlySpan<byte> source) => HashCore(source);
	public void HashCore(ushort[] array, int ibStart, int cbSize) => HashCore(array.AsSpan(ibStart, cbSize));
	public void HashCore(uint[] array, int ibStart, int cbSize) => HashCore(array.AsSpan(ibStart, cbSize));
	public void HashCore(ulong[] array, int ibStart, int cbSize) => HashCore(array.AsSpan(ibStart, cbSize));

	public void Reset(T value) => Value = value;
	public override void Reset() => Value = Basis;
	public override void Initialize() => Reset(Basis);

	public override T GetValueFinal() {
		var val = Value;
		Reset();
		return val;
	}
}
