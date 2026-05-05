// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.Intrinsics;

namespace Charon;

public static class XorUtils {
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void Xor(Span<byte> left, byte right) {
		var vectorSize = Vector<byte>.Count;
		switch (left.Length) {
			case >= 512 when Vector512.IsHardwareAccelerated && vectorSize < 512:
				Xor512(left, right);
				return;
			case >= 256 when Vector256.IsHardwareAccelerated && vectorSize < 256:
				Xor256(left, right);
				return;
			case >= 128 when Vector128.IsHardwareAccelerated && vectorSize < 128:
				Xor128(left, right);
				return;
		}

		var dataSize = left.Length;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		fixed (byte* leftp = left) {
			var rightK = new Vector<byte>(right);
			for (var i = 0; i < dataSizeVector; i += vectorSize) {
				var leftK = Unsafe.Read<Vector<byte>>(leftp + i);
				Unsafe.Write(leftp + i, Vector.Xor(leftK, rightK));
			}

			for (var i = dataSizeVector; i < dataSize; i++) {
				leftp[i] ^= right;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void Xor(Span<byte> left, ReadOnlySpan<byte> right) {
		ArgumentOutOfRangeException.ThrowIfGreaterThan(left.Length, right.Length);

		var vectorSize = Vector<byte>.Count;
		switch (left.Length) {
			case >= 512 when Vector512.IsHardwareAccelerated && vectorSize < 512:
				Xor512(left, right);
				return;
			case >= 256 when Vector256.IsHardwareAccelerated && vectorSize < 256:
				Xor256(left, right);
				return;
			case >= 128 when Vector128.IsHardwareAccelerated && vectorSize < 128:
				Xor128(left, right);
				return;
		}

		var dataSize = left.Length;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		fixed (byte* leftp = left) {
			fixed (byte* rightp = right) {
				for (var i = 0; i < dataSizeVector; i += vectorSize) {
					var leftK = Unsafe.Read<Vector<byte>>(leftp + i);
					var rightK = Unsafe.Read<Vector<byte>>(rightp + i);
					Unsafe.Write(leftp + i, Vector.Xor(leftK, rightK));
				}

				for (var i = dataSizeVector; i < dataSize; i++) {
					leftp[i] ^= rightp[i];
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void XorUnsafe(byte* leftp, int dataSize, byte right) {
		var vectorSize = Vector<byte>.Count;
		switch (dataSize) {
			case >= 512 when Vector512.IsHardwareAccelerated && vectorSize < 512:
				XorUnsafe512(leftp, dataSize, right);
				return;
			case >= 256 when Vector256.IsHardwareAccelerated && vectorSize < 256:
				XorUnsafe256(leftp, dataSize, right);
				return;
			case >= 128 when Vector128.IsHardwareAccelerated && vectorSize < 128:
				XorUnsafe128(leftp, dataSize, right);
				return;
		}

		var dataSizeVector = dataSize - dataSize % vectorSize;
		var rightK = new Vector<byte>(right);
		for (var i = 0; i < dataSizeVector; i += vectorSize) {
			var leftK = Unsafe.Read<Vector<byte>>(leftp + i);
			Unsafe.Write(leftp + i, Vector.Xor(leftK, rightK));
		}

		for (var i = dataSizeVector; i < dataSize; i++) {
			leftp[i] ^= right;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void XorUnsafe(byte* leftp, int dataSize, byte* rightp, int rightSize) {
		ArgumentOutOfRangeException.ThrowIfGreaterThan(dataSize, rightSize);

		var vectorSize = Vector<byte>.Count;
		switch (dataSize) {
			case >= 512 when Vector512.IsHardwareAccelerated && vectorSize < 512:
				XorUnsafe512(leftp, dataSize, rightp, rightSize);
				return;
			case >= 256 when Vector256.IsHardwareAccelerated && vectorSize < 256:
				XorUnsafe256(leftp, dataSize, rightp, rightSize);
				return;
			case >= 128 when Vector128.IsHardwareAccelerated && vectorSize < 128:
				XorUnsafe128(leftp, dataSize, rightp, rightSize);
				return;
		}

		var dataSizeVector = dataSize - dataSize % vectorSize;
		for (var i = 0; i < dataSizeVector; i += vectorSize) {
			var leftK = Unsafe.Read<Vector<byte>>(leftp + i);
			var rightK = Unsafe.Read<Vector<byte>>(rightp + i);
			Unsafe.Write(leftp + i, Vector.Xor(leftK, rightK));
		}

		for (var i = dataSizeVector; i < dataSize; i++) {
			leftp[i] ^= rightp[i];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void Xor512(Span<byte> left, ReadOnlySpan<byte> right) {
		ArgumentOutOfRangeException.ThrowIfGreaterThan(left.Length, right.Length);

		var vectorSize = Vector512<byte>.Count;
		var dataSize = left.Length;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		fixed (byte* leftp = left) {
			fixed (byte* rightp = right) {
				for (var i = 0; i < dataSizeVector; i += vectorSize) {
					var leftK = Unsafe.Read<Vector512<byte>>(leftp + i);
					var rightK = Unsafe.Read<Vector512<byte>>(rightp + i);
					Unsafe.Write(leftp + i, Vector512.Xor(leftK, rightK));
				}

				for (var i = dataSizeVector; i < dataSize; i++) {
					leftp[i] ^= rightp[i];
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void Xor512(Span<byte> left, byte right) {
		var vectorSize = Vector512<byte>.Count;
		var dataSize = left.Length;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		fixed (byte* leftp = left) {
			var rightK = Vector512.CreateScalar(right);

			for (var i = 0; i < dataSizeVector; i += vectorSize) {
				var leftK = Unsafe.Read<Vector512<byte>>(leftp + i);
				Unsafe.Write(leftp + i, Vector512.Xor(leftK, rightK));
			}

			for (var i = dataSizeVector; i < dataSize; i++) {
				leftp[i] ^= right;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void XorUnsafe512(byte* leftp, int dataSize, byte* rightp, int rightSize) {
		ArgumentOutOfRangeException.ThrowIfGreaterThan(dataSize, rightSize);

		var vectorSize = Vector512<byte>.Count;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		for (var i = 0; i < dataSizeVector; i += vectorSize) {
			var leftK = Unsafe.Read<Vector512<byte>>(leftp + i);
			var rightK = Unsafe.Read<Vector512<byte>>(rightp + i);
			Unsafe.Write(leftp + i, Vector512.Xor(leftK, rightK));
		}

		for (var i = dataSizeVector; i < dataSize; i++) {
			leftp[i] ^= rightp[i];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void XorUnsafe512(byte* leftp, int dataSize, byte right) {
		var vectorSize = Vector512<byte>.Count;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		var rightK = Vector512.CreateScalar(right);
		for (var i = 0; i < dataSizeVector; i += vectorSize) {
			var leftK = Unsafe.Read<Vector512<byte>>(leftp + i);
			Unsafe.Write(leftp + i, Vector512.Xor(leftK, rightK));
		}

		for (var i = dataSizeVector; i < dataSize; i++) {
			leftp[i] ^= right;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void Xor256(Span<byte> left, byte right) {
		var vectorSize = Vector256<byte>.Count;
		var dataSize = left.Length;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		fixed (byte* leftp = left) {
			var rightK = Vector256.CreateScalar(right);

			for (var i = 0; i < dataSizeVector; i += vectorSize) {
				var leftK = Unsafe.Read<Vector256<byte>>(leftp + i);
				Unsafe.Write(leftp + i, Vector256.Xor(leftK, rightK));
			}

			for (var i = dataSizeVector; i < dataSize; i++) {
				leftp[i] ^= right;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void Xor256(Span<byte> left, ReadOnlySpan<byte> right) {
		ArgumentOutOfRangeException.ThrowIfGreaterThan(left.Length, right.Length);

		var vectorSize = Vector256<byte>.Count;
		var dataSize = left.Length;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		fixed (byte* leftp = left) {
			fixed (byte* rightp = right) {
				for (var i = 0; i < dataSizeVector; i += vectorSize) {
					var leftK = Unsafe.Read<Vector256<byte>>(leftp + i);
					var rightK = Unsafe.Read<Vector256<byte>>(rightp + i);
					Unsafe.Write(leftp + i, Vector256.Xor(leftK, rightK));
				}

				for (var i = dataSizeVector; i < dataSize; i++) {
					leftp[i] ^= rightp[i];
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void XorUnsafe256(byte* leftp, int dataSize, byte* rightp, int rightSize) {
		ArgumentOutOfRangeException.ThrowIfGreaterThan(dataSize, rightSize);

		var vectorSize = Vector256<byte>.Count;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		for (var i = 0; i < dataSizeVector; i += vectorSize) {
			var leftK = Unsafe.Read<Vector256<byte>>(leftp + i);
			var rightK = Unsafe.Read<Vector256<byte>>(rightp + i);
			Unsafe.Write(leftp + i, Vector256.Xor(leftK, rightK));
		}

		for (var i = dataSizeVector; i < dataSize; i++) {
			leftp[i] ^= rightp[i];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void XorUnsafe256(byte* leftp, int dataSize, byte right) {
		var vectorSize = Vector256<byte>.Count;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		var rightK = Vector256.CreateScalar(right);
		for (var i = 0; i < dataSizeVector; i += vectorSize) {
			var leftK = Unsafe.Read<Vector256<byte>>(leftp + i);
			Unsafe.Write(leftp + i, Vector256.Xor(leftK, rightK));
		}

		for (var i = dataSizeVector; i < dataSize; i++) {
			leftp[i] ^= right;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void Xor128(Span<byte> left, byte right) {
		var vectorSize = Vector128<byte>.Count;
		var dataSize = left.Length;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		fixed (byte* leftp = left) {
			var rightK = Vector128.CreateScalar(right);

			for (var i = 0; i < dataSizeVector; i += vectorSize) {
				var leftK = Unsafe.Read<Vector128<byte>>(leftp + i);
				Unsafe.Write(leftp + i, Vector128.Xor(leftK, rightK));
			}

			for (var i = dataSizeVector; i < dataSize; i++) {
				leftp[i] ^= right;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void Xor128(Span<byte> left, ReadOnlySpan<byte> right) {
		ArgumentOutOfRangeException.ThrowIfGreaterThan(left.Length, right.Length);

		var vectorSize = Vector128<byte>.Count;
		var dataSize = left.Length;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		fixed (byte* leftp = left) {
			fixed (byte* rightp = right) {
				for (var i = 0; i < dataSizeVector; i += vectorSize) {
					var leftK = Unsafe.Read<Vector128<byte>>(leftp + i);
					var rightK = Unsafe.Read<Vector128<byte>>(rightp + i);
					Unsafe.Write(leftp + i, Vector128.Xor(leftK, rightK));
				}

				for (var i = dataSizeVector; i < dataSize; i++) {
					leftp[i] ^= rightp[i];
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void XorUnsafe128(byte* leftp, int dataSize, byte* rightp, int rightSize) {
		ArgumentOutOfRangeException.ThrowIfGreaterThan(dataSize, rightSize);

		var vectorSize = Vector128<byte>.Count;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		for (var i = 0; i < dataSizeVector; i += vectorSize) {
			var leftK = Unsafe.Read<Vector128<byte>>(leftp + i);
			var rightK = Unsafe.Read<Vector128<byte>>(rightp + i);
			Unsafe.Write(leftp + i, Vector128.Xor(leftK, rightK));
		}

		for (var i = dataSizeVector; i < dataSize; i++) {
			leftp[i] ^= rightp[i];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static unsafe void XorUnsafe128(byte* leftp, int dataSize, byte right) {
		var vectorSize = Vector128<byte>.Count;
		var dataSizeVector = dataSize - dataSize % vectorSize;
		var rightK = Vector128.CreateScalar(right);
		for (var i = 0; i < dataSizeVector; i += vectorSize) {
			var leftK = Unsafe.Read<Vector128<byte>>(leftp + i);
			Unsafe.Write(leftp + i, Vector128.Xor(leftK, rightK));
		}

		for (var i = dataSizeVector; i < dataSize; i++) {
			leftp[i] ^= right;
		}
	}
}
