// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Triton.Pixel;

[SuppressMessage("ReSharper", "InvertIf")]
public static class NumericConversion {
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static T GetMinimumValueSafe<T>() where T : unmanaged, INumberBase<T>, IMinMaxValue<T> =>
		typeof(T) == typeof(Half) || typeof(T) == typeof(float) || typeof(T) == typeof(NFloat) || typeof(T) == typeof(double) || typeof(T) == typeof(decimal)
			? T.Zero
			: T.MinValue;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal static T GetMaximumValueSafe<T>() where T : unmanaged, INumberBase<T>, IMinMaxValue<T> =>
		typeof(T) == typeof(Half) || typeof(T) == typeof(float) || typeof(T) == typeof(NFloat) || typeof(T) == typeof(double) || typeof(T) == typeof(decimal)
			? T.One
			: T.MaxValue;

#if DEBUG
	[DoesNotReturn]
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static T ThrowOrReturnDefault<T>() where T : struct {
	#if DEBUG
		throw new InvalidCastException();
	#else
		return default; //exceptions prevent inlining
	#endif
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ToSignedNumber<TFrom, TTo>(TFrom value)
		where TFrom : unmanaged, IUnsignedNumber<TFrom>, IShiftOperators<TFrom, int, TFrom>, IBitwiseOperators<TFrom, TFrom, TFrom>
		where TTo : unmanaged, ISignedNumber<TTo> {
		if (Unsafe.SizeOf<TFrom>() != Unsafe.SizeOf<TTo>()) {
			return ThrowOrReturnDefault<TTo>();
		}

		var signBit = TFrom.One << (Unsafe.SizeOf<TFrom>() * 8 - 1);
		var converted = signBit ^ value;
		return Unsafe.As<TFrom, TTo>(ref converted);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ToUnsignedNumber<TFrom, TTo>(TFrom value)
		where TFrom : unmanaged, ISignedNumber<TFrom>
		where TTo : unmanaged, IUnsignedNumber<TTo>, IShiftOperators<TTo, int, TTo>, IBitwiseOperators<TTo, TTo, TTo> {
		if (Unsafe.SizeOf<TFrom>() != Unsafe.SizeOf<TTo>()) {
			return ThrowOrReturnDefault<TTo>();
		}

		var signBit = TTo.One << (Unsafe.SizeOf<TTo>() * 8 - 1);
		return signBit ^ Unsafe.As<TFrom, TTo>(ref value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static TTo Convert<TFrom, TTo>(TFrom value) where TFrom : unmanaged where TTo : unmanaged {
		if (typeof(TFrom) == typeof(TTo)) {
			return Unsafe.As<TFrom, TTo>(ref value);
		}

		if (typeof(TFrom) == typeof(sbyte)) {
			return ConvertSByte<TTo>(Unsafe.As<TFrom, sbyte>(ref value));
		}

		if (typeof(TFrom) == typeof(byte)) {
			return ConvertByte<TTo>(Unsafe.As<TFrom, byte>(ref value));
		}

		if (typeof(TFrom) == typeof(short)) {
			return ConvertInt16<TTo>(Unsafe.As<TFrom, short>(ref value));
		}

		if (typeof(TFrom) == typeof(ushort)) {
			return ConvertUInt16<TTo>(Unsafe.As<TFrom, ushort>(ref value));
		}

		if (typeof(TFrom) == typeof(int)) {
			return ConvertInt32<TTo>(Unsafe.As<TFrom, int>(ref value));
		}

		if (typeof(TFrom) == typeof(uint)) {
			return ConvertUInt32<TTo>(Unsafe.As<TFrom, uint>(ref value));
		}

		if (typeof(TFrom) == typeof(nint)) {
			return ConvertIntPtr<TTo>(Unsafe.As<TFrom, nint>(ref value));
		}

		if (typeof(TFrom) == typeof(nuint)) {
			return ConvertUIntPtr<TTo>(Unsafe.As<TFrom, nuint>(ref value));
		}

		if (typeof(TFrom) == typeof(long)) {
			return ConvertInt64<TTo>(Unsafe.As<TFrom, long>(ref value));
		}

		if (typeof(TFrom) == typeof(ulong)) {
			return ConvertUInt64<TTo>(Unsafe.As<TFrom, ulong>(ref value));
		}

		if (typeof(TFrom) == typeof(Int128)) {
			return ConvertInt128<TTo>(Unsafe.As<TFrom, Int128>(ref value));
		}

		if (typeof(TFrom) == typeof(UInt128)) {
			return ConvertUInt128<TTo>(Unsafe.As<TFrom, UInt128>(ref value));
		}

		if (typeof(TFrom) == typeof(Half)) {
			return ConvertHalf<TTo>(Unsafe.As<TFrom, Half>(ref value));
		}

		if (typeof(TFrom) == typeof(float)) {
			return ConvertSingle<TTo>(Unsafe.As<TFrom, float>(ref value));
		}

		if (typeof(TFrom) == typeof(NFloat)) {
			return ConvertNFloat<TTo>(Unsafe.As<TFrom, NFloat>(ref value));
		}

		if (typeof(TFrom) == typeof(double)) {
			return ConvertDouble<TTo>(Unsafe.As<TFrom, double>(ref value));
		}

		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (typeof(TFrom) == typeof(decimal)) {
			return ConvertDecimal<TTo>(Unsafe.As<TFrom, decimal>(ref value));
		}

		return default;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertSByte<TTo>(sbyte value) where TTo : unmanaged => ConvertByte<TTo>(ChangeSign(value));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertByte<TTo>(byte value) where TTo : unmanaged {
		if (typeof(TTo) == typeof(sbyte)) {
			var converted = ChangeSign(ConvertByte<byte>(value));
			return Unsafe.As<sbyte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(byte)) {
			return Unsafe.As<byte, TTo>(ref value);
		}

		if (typeof(TTo) == typeof(short)) {
			var converted = ChangeSign(ConvertByte<ushort>(value));
			return Unsafe.As<short, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ushort)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			unchecked {
				var converted = (ushort) (((uint) value << 8) | value);
				return Unsafe.As<ushort, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(int)) {
			var converted = ChangeSign(ConvertByte<uint>(value));
			return Unsafe.As<int, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(uint)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			var converted = ((uint) value << 24) | ((uint) value << 16) | ((uint) value << 8) | value;
			return Unsafe.As<uint, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(nint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nint) ConvertByte<int>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			} else {
				var converted = (nint) ConvertByte<long>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(nuint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nuint) ConvertByte<uint>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			} else {
				var converted = (nuint) ConvertByte<ulong>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(long)) {
			var converted = ChangeSign(ConvertByte<ulong>(value));
			return Unsafe.As<long, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ulong)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			var converted = ((ulong) value << 56) | ((ulong) value << 48) | ((ulong) value << 40) | ((ulong) value << 32) | ((ulong) value << 24) | ((ulong) value << 16) | ((ulong) value << 8) | value;
			return Unsafe.As<ulong, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Int128)) {
			var converted = ChangeSign(ConvertByte<UInt128>(value));
			return Unsafe.As<Int128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(UInt128)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			var converted = ((UInt128) value << 120) | ((UInt128) value << 112) | ((UInt128) value << 104) | ((UInt128) value << 96) | ((UInt128) value << 88) | ((UInt128) value << 80) | ((UInt128) value << 72) | ((UInt128) value << 64) | ((UInt128) value << 56) | ((UInt128) value << 48) | ((UInt128) value << 40) | ((UInt128) value << 32) | ((UInt128) value << 24) | ((UInt128) value << 16) | ((UInt128) value << 8) | value;
			return Unsafe.As<UInt128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Half)) {
			// There isn't enough precision to convert from anything bigger than byte to Half, so we convert to float first.
			var x = ConvertByte<float>(value);
			var converted = (Half) x;
			return Unsafe.As<Half, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(float)) {
			var converted = value / (float) byte.MaxValue;
			return Unsafe.As<float, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(NFloat)) {
			if (nint.Size == sizeof(int)) {
				var converted = (NFloat) ConvertByte<float>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			} else {
				var converted = (NFloat) ConvertByte<double>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(double)) {
			var converted = value / (double) byte.MaxValue;
			return Unsafe.As<double, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(decimal)) {
			var converted = value / (decimal) byte.MaxValue;
			return Unsafe.As<decimal, TTo>(ref converted);
		}

		return default;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertInt16<TTo>(short value) where TTo : unmanaged => ConvertUInt16<TTo>(ChangeSign(value));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertUInt16<TTo>(ushort value) where TTo : unmanaged {
		if (typeof(TTo) == typeof(sbyte)) {
			var converted = ChangeSign(ConvertUInt16<byte>(value));
			return Unsafe.As<sbyte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(byte)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			// This is a special case where we already know an optimal algorithm.
			var x = (value * 255u + 32895u) >> 16;
			var converted = unchecked((byte) x);
			return Unsafe.As<byte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(short)) {
			var converted = ChangeSign(ConvertUInt16<ushort>(value));
			return Unsafe.As<short, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ushort)) {
			return Unsafe.As<ushort, TTo>(ref value);
		}

		if (typeof(TTo) == typeof(int)) {
			var converted = ChangeSign(ConvertUInt16<uint>(value));
			return Unsafe.As<int, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(uint)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			var converted = ((uint) value << 16) | value;
			return Unsafe.As<uint, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(nint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nint) ConvertUInt16<int>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			} else {
				var converted = (nint) ConvertUInt16<long>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(nuint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nuint) ConvertUInt16<uint>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			} else {
				var converted = (nuint) ConvertUInt16<ulong>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(long)) {
			var converted = ChangeSign(ConvertUInt16<ulong>(value));
			return Unsafe.As<long, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ulong)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			var converted = ((ulong) value << 48) | ((ulong) value << 32) | ((ulong) value << 16) | value;
			return Unsafe.As<ulong, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Int128)) {
			var converted = ChangeSign(ConvertUInt16<UInt128>(value));
			return Unsafe.As<Int128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(UInt128)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			var converted = ((UInt128) value << 112) | ((UInt128) value << 96) | ((UInt128) value << 80) | ((UInt128) value << 64) | ((UInt128) value << 48) | ((UInt128) value << 32) | ((UInt128) value << 16) | value;
			return Unsafe.As<UInt128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Half)) {
			// There isn't enough precision to convert from anything bigger than byte to Half, so we convert to float first.
			var x = ConvertUInt16<float>(value);
			var converted = (Half) x;
			return Unsafe.As<Half, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(float)) {
			var converted = value / (float) ushort.MaxValue;
			return Unsafe.As<float, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(NFloat)) {
			if (nint.Size == sizeof(int)) {
				var converted = (NFloat) ConvertUInt16<float>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			} else {
				var converted = (NFloat) ConvertUInt16<double>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(double)) {
			var converted = value / (double) ushort.MaxValue;
			return Unsafe.As<double, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(decimal)) {
			var converted = value / (decimal) ushort.MaxValue;
			return Unsafe.As<decimal, TTo>(ref converted);
		}

		return default;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertInt32<TTo>(int value) where TTo : unmanaged => ConvertUInt32<TTo>(ChangeSign(value));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertUInt32<TTo>(uint value) where TTo : unmanaged {
		if (typeof(TTo) == typeof(sbyte)) {
			var converted = ChangeSign(ConvertUInt32<byte>(value));
			return Unsafe.As<sbyte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(byte)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			// There are more accurate ways to map UInt32 onto Byte, but this is the simplest.
			unchecked {
				var converted = (byte) (value >> 24);
				return Unsafe.As<byte, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(short)) {
			var converted = ChangeSign(ConvertUInt32<ushort>(value));
			return Unsafe.As<short, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ushort)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			// There are more accurate ways to map UInt32 onto UInt16, but this is the simplest.
			unchecked {
				var converted = (ushort) (value >> 16);
				return Unsafe.As<ushort, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(int)) {
			var converted = ChangeSign(ConvertUInt32<uint>(value));
			return Unsafe.As<int, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(uint)) {
			return Unsafe.As<uint, TTo>(ref value);
		}

		if (typeof(TTo) == typeof(nint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nint) ConvertUInt32<int>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			} else {
				var converted = (nint) ConvertUInt32<long>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(nuint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nuint) ConvertUInt32<uint>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			} else {
				var converted = (nuint) ConvertUInt32<ulong>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(long)) {
			var converted = ChangeSign(ConvertUInt32<ulong>(value));
			return Unsafe.As<long, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ulong)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			var converted = ((ulong) value << 32) | value;
			return Unsafe.As<ulong, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Int128)) {
			var converted = ChangeSign(ConvertUInt32<UInt128>(value));
			return Unsafe.As<Int128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(UInt128)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			var converted = ((UInt128) value << 96) | ((UInt128) value << 64) | ((UInt128) value << 32) | value;
			return Unsafe.As<UInt128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Half)) {
			// There isn't enough precision to convert from anything bigger than byte to Half, so we convert to float first.
			var x = ConvertUInt32<float>(value);
			var converted = (Half) x;
			return Unsafe.As<Half, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(float)) {
			var converted = value / (float) uint.MaxValue;
			return Unsafe.As<float, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(NFloat)) {
			if (nint.Size == sizeof(int)) {
				var converted = (NFloat) ConvertUInt32<float>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			} else {
				var converted = (NFloat) ConvertUInt32<double>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(double)) {
			var converted = value / (double) uint.MaxValue;
			return Unsafe.As<double, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(decimal)) {
			var converted = value / (decimal) uint.MaxValue;
			return Unsafe.As<decimal, TTo>(ref converted);
		}

		return default;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertIntPtr<TTo>(nint value) where TTo : unmanaged => nint.Size == sizeof(int) ? ConvertInt32<TTo>((int) value) : ConvertInt64<TTo>(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertUIntPtr<TTo>(nuint value) where TTo : unmanaged => nint.Size == sizeof(int) ? ConvertUInt32<TTo>((uint) value) : ConvertUInt64<TTo>(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertInt64<TTo>(long value) where TTo : unmanaged => ConvertUInt64<TTo>(ChangeSign(value));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertUInt64<TTo>(ulong value) where TTo : unmanaged {
		if (typeof(TTo) == typeof(sbyte)) {
			var converted = ChangeSign(ConvertUInt64<byte>(value));
			return Unsafe.As<sbyte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(byte)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			// There are more accurate ways to map UInt64 onto Byte, but this is the simplest.
			unchecked {
				var converted = (byte) (value >> 56);
				return Unsafe.As<byte, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(short)) {
			var converted = ChangeSign(ConvertUInt64<ushort>(value));
			return Unsafe.As<short, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ushort)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			// There are more accurate ways to map UInt64 onto UInt16, but this is the simplest.
			unchecked {
				var converted = (ushort) (value >> 48);
				return Unsafe.As<ushort, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(int)) {
			var converted = ChangeSign(ConvertUInt64<uint>(value));
			return Unsafe.As<int, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(uint)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			// There are more accurate ways to map UInt64 onto UInt32, but this is the simplest.
			unchecked {
				var converted = (uint) (value >> 32);
				return Unsafe.As<uint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(nint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nint) ConvertUInt64<int>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			} else {
				var converted = (nint) ConvertUInt64<long>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(nuint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nuint) ConvertUInt64<uint>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			} else {
				var converted = (nuint) ConvertUInt64<ulong>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(long)) {
			var converted = ChangeSign(ConvertUInt64<ulong>(value));
			return Unsafe.As<long, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ulong)) {
			return Unsafe.As<ulong, TTo>(ref value);
		}

		if (typeof(TTo) == typeof(Int128)) {
			var converted = ChangeSign(ConvertUInt64<UInt128>(value));
			return Unsafe.As<Int128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(UInt128)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			var converted = ((UInt128) value << 64) | value;
			return Unsafe.As<UInt128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Half)) {
			// There isn't enough precision to convert from anything bigger than byte to Half, so we convert to float first.
			var x = ConvertUInt64<float>(value);
			var converted = (Half) x;
			return Unsafe.As<Half, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(float)) {
			var converted = value / (float) ulong.MaxValue;
			return Unsafe.As<float, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(NFloat)) {
			if (nint.Size == sizeof(int)) {
				var converted = (NFloat) ConvertUInt64<float>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			} else {
				var converted = (NFloat) ConvertUInt64<double>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(double)) {
			var converted = value / (double) ulong.MaxValue;
			return Unsafe.As<double, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(decimal)) {
			var converted = value / (decimal) ulong.MaxValue;
			return Unsafe.As<decimal, TTo>(ref converted);
		}

		return default;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertInt128<TTo>(Int128 value) where TTo : unmanaged => ConvertUInt128<TTo>(ChangeSign(value));

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertUInt128<TTo>(UInt128 value) where TTo : unmanaged {
		if (typeof(TTo) == typeof(sbyte)) {
			var converted = ChangeSign(ConvertUInt128<byte>(value));
			return Unsafe.As<sbyte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(byte)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			// There are more accurate ways to map UInt128 onto Byte, but this is the simplest.
			unchecked {
				var converted = (byte) (value >> 120);
				return Unsafe.As<byte, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(short)) {
			var converted = ChangeSign(ConvertUInt128<ushort>(value));
			return Unsafe.As<short, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ushort)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			// There are more accurate ways to map UInt128 onto UInt16, but this is the simplest.
			unchecked {
				var converted = (ushort) (value >> 112);
				return Unsafe.As<ushort, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(int)) {
			var converted = ChangeSign(ConvertUInt128<uint>(value));
			return Unsafe.As<int, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(uint)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			// There are more accurate ways to map UInt128 onto UInt32, but this is the simplest.
			unchecked {
				var converted = (uint) (value >> 96);
				return Unsafe.As<uint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(nint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nint) ConvertUInt128<int>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			} else {
				var converted = (nint) ConvertUInt128<long>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(nuint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nuint) ConvertUInt128<uint>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			} else {
				var converted = (nuint) ConvertUInt128<ulong>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(long)) {
			var converted = ChangeSign(ConvertUInt128<ulong>(value));
			return Unsafe.As<long, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ulong)) {
			// See https://github.com/AssetRipper/TextureDecoder/issues/19
			// There are more accurate ways to map UInt128 onto UInt64, but this is the simplest.
			unchecked {
				var converted = (ulong) (value >> 64);
				return Unsafe.As<ulong, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(Int128)) {
			var converted = ChangeSign(ConvertUInt128<UInt128>(value));
			return Unsafe.As<Int128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(UInt128)) {
			return Unsafe.As<UInt128, TTo>(ref value);
		}

		if (typeof(TTo) == typeof(Half)) {
			// There isn't enough precision to convert from anything bigger than byte to Half, so we convert to float first.
			var x = ConvertUInt128<float>(value);
			var converted = (Half) x;
			return Unsafe.As<Half, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(float)) {
			var converted = (float) value / (float) UInt128.MaxValue;
			return Unsafe.As<float, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(NFloat)) {
			if (nint.Size == sizeof(int)) {
				var converted = (NFloat) ConvertUInt128<float>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			} else {
				var converted = (NFloat) ConvertUInt128<double>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(double)) {
			var converted = (double) value / (double) UInt128.MaxValue;
			return Unsafe.As<double, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(decimal)) {
			var converted = (decimal) value / (decimal) UInt128.MaxValue;
			return Unsafe.As<decimal, TTo>(ref converted);
		}

		return default;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertHalf<TTo>(Half value) where TTo : unmanaged {
		if (typeof(TTo) == typeof(sbyte)) {
			var converted = ChangeSign(ConvertHalf<byte>(value));
			return Unsafe.As<sbyte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(byte)) {
			// We use float because it has enough precision to convert from Half to any integer type.
			return ConvertSingle<TTo>((float) value);
		}

		if (typeof(TTo) == typeof(short)) {
			var converted = ChangeSign(ConvertHalf<ushort>(value));
			return Unsafe.As<short, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ushort)) {
			// We use float because it has enough precision to convert from Half to any integer type.
			return ConvertSingle<TTo>((float) value);
		}

		if (typeof(TTo) == typeof(int)) {
			var converted = ChangeSign(ConvertHalf<uint>(value));
			return Unsafe.As<int, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(uint)) {
			// We use float because it has enough precision to convert from Half to any integer type.
			return ConvertSingle<TTo>((float) value);
		}

		if (typeof(TTo) == typeof(nint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nint) ConvertHalf<int>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			} else {
				var converted = (nint) ConvertHalf<long>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(nuint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nuint) ConvertHalf<uint>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			} else {
				var converted = (nuint) ConvertHalf<ulong>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(long)) {
			var converted = ChangeSign(ConvertHalf<ulong>(value));
			return Unsafe.As<long, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ulong)) {
			// We use float because it has enough precision to convert from Half to any integer type.
			return ConvertSingle<TTo>((float) value);
		}

		if (typeof(TTo) == typeof(Int128)) {
			var converted = ChangeSign(ConvertHalf<UInt128>(value));
			return Unsafe.As<Int128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(UInt128)) {
			// We use float because it has enough precision to convert from Half to any integer type.
			return ConvertSingle<TTo>((float) value);
		}

		if (typeof(TTo) == typeof(Half)) {
			return Unsafe.As<Half, TTo>(ref value);
		}

		if (typeof(TTo) == typeof(float)) {
			var converted = (float) value;
			return Unsafe.As<float, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(NFloat)) {
			if (nint.Size == sizeof(int)) {
				var converted = (NFloat) ConvertHalf<float>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			} else {
				var converted = (NFloat) ConvertHalf<double>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(double)) {
			var converted = (double) value;
			return Unsafe.As<double, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(decimal)) {
			var converted = (decimal) value;
			return Unsafe.As<decimal, TTo>(ref converted);
		}

		return default;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertSingle<TTo>(float value) where TTo : unmanaged {
		if (typeof(TTo) == typeof(sbyte)) {
			var converted = ChangeSign(ConvertSingle<byte>(value));
			return Unsafe.As<sbyte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(byte)) {
			// x must be clamped because of rounding errors.
			var x = value * byte.MaxValue;
			var converted = byte.MaxValue < x ? byte.MaxValue : x > byte.MinValue ? (byte) x : byte.MinValue;
			return Unsafe.As<byte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(short)) {
			var converted = ChangeSign(ConvertSingle<ushort>(value));
			return Unsafe.As<short, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ushort)) {
			// x must be clamped because of rounding errors.
			var x = value * ushort.MaxValue;
			var converted = ushort.MaxValue < x ? ushort.MaxValue : x > ushort.MinValue ? (ushort) x : ushort.MinValue;
			return Unsafe.As<ushort, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(int)) {
			var converted = ChangeSign(ConvertSingle<uint>(value));
			return Unsafe.As<int, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(uint)) {
			// x must be clamped because of rounding errors.
			var x = value * uint.MaxValue;
			var converted = uint.MaxValue < x ? uint.MaxValue : x > uint.MinValue ? (uint) x : uint.MinValue;
			return Unsafe.As<uint, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(nint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nint) ConvertSingle<int>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			} else {
				var converted = (nint) ConvertSingle<long>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(nuint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nuint) ConvertSingle<uint>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			} else {
				var converted = (nuint) ConvertSingle<ulong>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(long)) {
			var converted = ChangeSign(ConvertSingle<ulong>(value));
			return Unsafe.As<long, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ulong)) {
			// x must be clamped because of rounding errors.
			var x = value * ulong.MaxValue;
			var converted = ulong.MaxValue < x ? ulong.MaxValue : x > ulong.MinValue ? (ulong) x : ulong.MinValue;
			return Unsafe.As<ulong, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Int128)) {
			var converted = ChangeSign(ConvertSingle<UInt128>(value));
			return Unsafe.As<Int128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(UInt128)) {
			// x must be clamped because of rounding errors.
			var x = value * (float) UInt128.MaxValue;
			var converted = (float) UInt128.MaxValue < x ? UInt128.MaxValue : x > (float) UInt128.MinValue ? (UInt128) x : UInt128.MinValue;
			return Unsafe.As<UInt128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Half)) {
			var converted = (Half) value;
			return Unsafe.As<Half, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(float)) {
			return Unsafe.As<float, TTo>(ref value);
		}

		if (typeof(TTo) == typeof(NFloat)) {
			if (nint.Size == sizeof(int)) {
				var converted = (NFloat) ConvertSingle<float>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			} else {
				var converted = (NFloat) ConvertSingle<double>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(double)) {
			var converted = (double) value;
			return Unsafe.As<double, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(decimal)) {
			var converted = (decimal) value;
			return Unsafe.As<decimal, TTo>(ref converted);
		}

		return default;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertNFloat<TTo>(NFloat value) where TTo : unmanaged => nint.Size == sizeof(int) ? ConvertSingle<TTo>((float) value) : ConvertDouble<TTo>(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertDouble<TTo>(double value) where TTo : unmanaged {
		if (typeof(TTo) == typeof(sbyte)) {
			var converted = ChangeSign(ConvertDouble<byte>(value));
			return Unsafe.As<sbyte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(byte)) {
			// x must be clamped because of rounding errors.
			var x = value * byte.MaxValue;
			var converted = byte.MaxValue < x ? byte.MaxValue : x > byte.MinValue ? (byte) x : byte.MinValue;
			return Unsafe.As<byte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(short)) {
			var converted = ChangeSign(ConvertDouble<ushort>(value));
			return Unsafe.As<short, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ushort)) {
			// x must be clamped because of rounding errors.
			var x = value * ushort.MaxValue;
			var converted = ushort.MaxValue < x ? ushort.MaxValue : x > ushort.MinValue ? (ushort) x : ushort.MinValue;
			return Unsafe.As<ushort, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(int)) {
			var converted = ChangeSign(ConvertDouble<uint>(value));
			return Unsafe.As<int, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(uint)) {
			// x must be clamped because of rounding errors.
			var x = value * uint.MaxValue;
			var converted = uint.MaxValue < x ? uint.MaxValue : x > uint.MinValue ? (uint) x : uint.MinValue;
			return Unsafe.As<uint, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(nint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nint) ConvertDouble<int>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			} else {
				var converted = (nint) ConvertDouble<long>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(nuint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nuint) ConvertDouble<uint>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			} else {
				var converted = (nuint) ConvertDouble<ulong>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(long)) {
			var converted = ChangeSign(ConvertDouble<ulong>(value));
			return Unsafe.As<long, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ulong)) {
			// x must be clamped because of rounding errors.
			var x = value * ulong.MaxValue;
			var converted = ulong.MaxValue < x ? ulong.MaxValue : x > ulong.MinValue ? (ulong) x : ulong.MinValue;
			return Unsafe.As<ulong, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Int128)) {
			var converted = ChangeSign(ConvertDouble<UInt128>(value));
			return Unsafe.As<Int128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(UInt128)) {
			// x must be clamped because of rounding errors.
			var x = value * (double) UInt128.MaxValue;
			var converted = (double) UInt128.MaxValue < x ? UInt128.MaxValue : x > (double) UInt128.MinValue ? (UInt128) x : UInt128.MinValue;
			return Unsafe.As<UInt128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Half)) {
			var converted = (Half) value;
			return Unsafe.As<Half, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(float)) {
			var converted = (float) value;
			return Unsafe.As<float, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(NFloat)) {
			if (nint.Size == sizeof(int)) {
				var converted = (NFloat) ConvertDouble<float>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			} else {
				var converted = (NFloat) ConvertDouble<double>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(double)) {
			return Unsafe.As<double, TTo>(ref value);
		}

		if (typeof(TTo) == typeof(decimal)) {
			var converted = (decimal) value;
			return Unsafe.As<decimal, TTo>(ref converted);
		}

		return default;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static TTo ConvertDecimal<TTo>(decimal value) where TTo : unmanaged {
		if (typeof(TTo) == typeof(sbyte)) {
			var converted = ChangeSign(ConvertDecimal<byte>(value));
			return Unsafe.As<sbyte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(byte)) {
			// x must be clamped because of rounding errors.
			var x = value * byte.MaxValue;
			var converted = byte.MaxValue < x ? byte.MaxValue : x > byte.MinValue ? (byte) x : byte.MinValue;
			return Unsafe.As<byte, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(short)) {
			var converted = ChangeSign(ConvertDecimal<ushort>(value));
			return Unsafe.As<short, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ushort)) {
			// x must be clamped because of rounding errors.
			var x = value * ushort.MaxValue;
			var converted = ushort.MaxValue < x ? ushort.MaxValue : x > ushort.MinValue ? (ushort) x : ushort.MinValue;
			return Unsafe.As<ushort, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(int)) {
			var converted = ChangeSign(ConvertDecimal<uint>(value));
			return Unsafe.As<int, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(uint)) {
			// x must be clamped because of rounding errors.
			var x = value * uint.MaxValue;
			var converted = uint.MaxValue < x ? uint.MaxValue : x > uint.MinValue ? (uint) x : uint.MinValue;
			return Unsafe.As<uint, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(nint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nint) ConvertDecimal<int>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			} else {
				var converted = (nint) ConvertDecimal<long>(value);
				return Unsafe.As<nint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(nuint)) {
			if (nint.Size == sizeof(int)) {
				var converted = (nuint) ConvertDecimal<uint>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			} else {
				var converted = (nuint) ConvertDecimal<ulong>(value);
				return Unsafe.As<nuint, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(long)) {
			var converted = ChangeSign(ConvertDecimal<ulong>(value));
			return Unsafe.As<long, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(ulong)) {
			// x must be clamped because of rounding errors.
			var x = value * ulong.MaxValue;
			var converted = ulong.MaxValue < x ? ulong.MaxValue : x > ulong.MinValue ? (ulong) x : ulong.MinValue;
			return Unsafe.As<ulong, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Int128)) {
			var converted = ChangeSign(ConvertDecimal<UInt128>(value));
			return Unsafe.As<Int128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(UInt128)) {
			// x must be clamped because of rounding errors.
			var x = value * (decimal) UInt128.MaxValue;
			var converted = (decimal) UInt128.MaxValue < x ? UInt128.MaxValue : x > (decimal) UInt128.MinValue ? (UInt128) x : UInt128.MinValue;
			return Unsafe.As<UInt128, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(Half)) {
			var converted = (Half) value;
			return Unsafe.As<Half, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(float)) {
			var converted = (float) value;
			return Unsafe.As<float, TTo>(ref converted);
		}

		if (typeof(TTo) == typeof(NFloat)) {
			if (nint.Size == sizeof(int)) {
				var converted = (NFloat) ConvertDecimal<float>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			} else {
				var converted = (NFloat) ConvertDecimal<double>(value);
				return Unsafe.As<NFloat, TTo>(ref converted);
			}
		}

		if (typeof(TTo) == typeof(double)) {
			var converted = (double) value;
			return Unsafe.As<double, TTo>(ref converted);
		}

		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (typeof(TTo) == typeof(decimal)) {
			return Unsafe.As<decimal, TTo>(ref value);
		}

		return default;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static T GetMinimumValue<T>() where T : unmanaged {
		if (typeof(T) == typeof(sbyte)) {
			var value = GetMinimumValueSafe<sbyte>();
			return Unsafe.As<sbyte, T>(ref value);
		}

		if (typeof(T) == typeof(byte)) {
			var value = GetMinimumValueSafe<byte>();
			return Unsafe.As<byte, T>(ref value);
		}

		if (typeof(T) == typeof(short)) {
			var value = GetMinimumValueSafe<short>();
			return Unsafe.As<short, T>(ref value);
		}

		if (typeof(T) == typeof(ushort)) {
			var value = GetMinimumValueSafe<ushort>();
			return Unsafe.As<ushort, T>(ref value);
		}

		if (typeof(T) == typeof(int)) {
			var value = GetMinimumValueSafe<int>();
			return Unsafe.As<int, T>(ref value);
		}

		if (typeof(T) == typeof(uint)) {
			var value = GetMinimumValueSafe<uint>();
			return Unsafe.As<uint, T>(ref value);
		}

		if (typeof(T) == typeof(nint)) {
			var value = GetMinimumValueSafe<nint>();
			return Unsafe.As<nint, T>(ref value);
		}

		if (typeof(T) == typeof(nuint)) {
			var value = GetMinimumValueSafe<nuint>();
			return Unsafe.As<nuint, T>(ref value);
		}

		if (typeof(T) == typeof(long)) {
			var value = GetMinimumValueSafe<long>();
			return Unsafe.As<long, T>(ref value);
		}

		if (typeof(T) == typeof(ulong)) {
			var value = GetMinimumValueSafe<ulong>();
			return Unsafe.As<ulong, T>(ref value);
		}

		if (typeof(T) == typeof(Int128)) {
			var value = GetMinimumValueSafe<Int128>();
			return Unsafe.As<Int128, T>(ref value);
		}

		if (typeof(T) == typeof(UInt128)) {
			var value = GetMinimumValueSafe<UInt128>();
			return Unsafe.As<UInt128, T>(ref value);
		}

		if (typeof(T) == typeof(Half)) {
			var value = GetMinimumValueSafe<Half>();
			return Unsafe.As<Half, T>(ref value);
		}

		if (typeof(T) == typeof(float)) {
			var value = GetMinimumValueSafe<float>();
			return Unsafe.As<float, T>(ref value);
		}

		if (typeof(T) == typeof(NFloat)) {
			var value = GetMinimumValueSafe<NFloat>();
			return Unsafe.As<NFloat, T>(ref value);
		}

		if (typeof(T) == typeof(double)) {
			var value = GetMinimumValueSafe<double>();
			return Unsafe.As<double, T>(ref value);
		}

		if (typeof(T) == typeof(decimal)) {
			var value = GetMinimumValueSafe<decimal>();
			return Unsafe.As<decimal, T>(ref value);
		}

		return ThrowOrReturnDefault<T>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static T GetMaximumValue<T>() where T : unmanaged {
		if (typeof(T) == typeof(sbyte)) {
			var value = GetMaximumValueSafe<sbyte>();
			return Unsafe.As<sbyte, T>(ref value);
		}

		if (typeof(T) == typeof(byte)) {
			var value = GetMaximumValueSafe<byte>();
			return Unsafe.As<byte, T>(ref value);
		}

		if (typeof(T) == typeof(short)) {
			var value = GetMaximumValueSafe<short>();
			return Unsafe.As<short, T>(ref value);
		}

		if (typeof(T) == typeof(ushort)) {
			var value = GetMaximumValueSafe<ushort>();
			return Unsafe.As<ushort, T>(ref value);
		}

		if (typeof(T) == typeof(int)) {
			var value = GetMaximumValueSafe<int>();
			return Unsafe.As<int, T>(ref value);
		}

		if (typeof(T) == typeof(uint)) {
			var value = GetMaximumValueSafe<uint>();
			return Unsafe.As<uint, T>(ref value);
		}

		if (typeof(T) == typeof(nint)) {
			var value = GetMaximumValueSafe<nint>();
			return Unsafe.As<nint, T>(ref value);
		}

		if (typeof(T) == typeof(nuint)) {
			var value = GetMaximumValueSafe<nuint>();
			return Unsafe.As<nuint, T>(ref value);
		}

		if (typeof(T) == typeof(long)) {
			var value = GetMaximumValueSafe<long>();
			return Unsafe.As<long, T>(ref value);
		}

		if (typeof(T) == typeof(ulong)) {
			var value = GetMaximumValueSafe<ulong>();
			return Unsafe.As<ulong, T>(ref value);
		}

		if (typeof(T) == typeof(Int128)) {
			var value = GetMaximumValueSafe<Int128>();
			return Unsafe.As<Int128, T>(ref value);
		}

		if (typeof(T) == typeof(UInt128)) {
			var value = GetMaximumValueSafe<UInt128>();
			return Unsafe.As<UInt128, T>(ref value);
		}

		if (typeof(T) == typeof(Half)) {
			var value = GetMaximumValueSafe<Half>();
			return Unsafe.As<Half, T>(ref value);
		}

		if (typeof(T) == typeof(float)) {
			var value = GetMaximumValueSafe<float>();
			return Unsafe.As<float, T>(ref value);
		}

		if (typeof(T) == typeof(NFloat)) {
			var value = GetMaximumValueSafe<NFloat>();
			return Unsafe.As<NFloat, T>(ref value);
		}

		if (typeof(T) == typeof(double)) {
			var value = GetMaximumValueSafe<double>();
			return Unsafe.As<double, T>(ref value);
		}

		if (typeof(T) == typeof(decimal)) {
			var value = GetMaximumValueSafe<decimal>();
			return Unsafe.As<decimal, T>(ref value);
		}

		return ThrowOrReturnDefault<T>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static sbyte ChangeSign(byte value) => ToSignedNumber<byte, sbyte>(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static byte ChangeSign(sbyte value) => ToUnsignedNumber<sbyte, byte>(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static short ChangeSign(ushort value) => ToSignedNumber<ushort, short>(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static ushort ChangeSign(short value) => ToUnsignedNumber<short, ushort>(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static int ChangeSign(uint value) => ToSignedNumber<uint, int>(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static uint ChangeSign(int value) => ToUnsignedNumber<int, uint>(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static long ChangeSign(ulong value) => ToSignedNumber<ulong, long>(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static ulong ChangeSign(long value) => ToUnsignedNumber<long, ulong>(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static Int128 ChangeSign(UInt128 value) => ToSignedNumber<UInt128, Int128>(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static UInt128 ChangeSign(Int128 value) => ToUnsignedNumber<Int128, UInt128>(value);
}
