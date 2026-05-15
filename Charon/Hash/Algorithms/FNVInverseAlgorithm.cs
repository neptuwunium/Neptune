// SPDX-FileCopyrightText: 2023-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;

namespace Charon.Hash.Algorithms;

// https://tools.ietf.org/html/draft-eastlake-fnv-17
// http://www.isthe.com/chongo/tech/comp/fnv/index.html
// ReSharper disable UnusedMethodReturnValue.Global
public sealed class FNVInverseAlgorithm<T> : SpanHashAlgorithm<T>
	where T : unmanaged, INumber<T>, IBitwiseOperators<T, T, T> {
	public const string FNV1_IV = @"chongo <Landon Curt Noll> /\../\";
	public const string FNV1B_IV = @"chongo (Landon Curt Noll) /\oo/\";
	private readonly T Basis;
	private readonly T Prime;

	public FNVInverseAlgorithm(T basis, T prime) {
		Basis = basis;
		Prime = prime;
		Reset(Basis);
	}

	public T HashNext(T value) {
		Value ^= value;
		Value *= Prime;
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
	public void HashCore(ulong[] array, int ibStart, int cbSize)  => HashCore(array.AsSpan(ibStart, cbSize));

	public void Reset(T value) => Value = value;
	public override void Reset() => Reset(Basis);
	public override void Initialize() => Reset(Basis);

	public override T GetValueFinal() {
		var val = Value;
		Reset();
		return val;
	}
	// ReSharper disable once InconsistentNaming
}
