// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;

namespace Triton.Pixel;

/// <summary>
///     An 8-bit struct containing a pair of 4-bit unsigned integers.
/// </summary>
internal struct UInt4Pair {
	private const uint HighBitMask = 0xF0u;
	private const uint LowBitMask = 0x0Fu;
	private byte Bits;

	/// <summary>
	///     The value stored in the high bits.
	/// </summary>
	/// <remarks>
	///     Possible values: any multiple of 17 in the inclusive range [0 - 255].
	/// </remarks>
	public byte HighValue {
		readonly get => (byte) ((Bits & HighBitMask) | ((uint) Bits >> 4));

		set {
			unchecked {
				Bits = (byte) ((Convert8BitsTo4Bits(value) << 4) | (Bits & LowBitMask));
			}
		}
	}

	/// <summary>
	///     The value stored in the low bits.
	/// </summary>
	/// <remarks>
	///     Possible values: any multiple of 17 in the inclusive range [0 - 255].
	/// </remarks>
	public byte LowValue {
		readonly get {
			var relevantBits = Bits & LowBitMask;
			return (byte) ((relevantBits << 4) | relevantBits);
		}

		set {
			unchecked {
				Bits = (byte) (Convert8BitsTo4Bits(value) | (Bits & HighBitMask));
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static uint Convert8BitsTo4Bits(byte value) =>
		//See https://github.com/AssetRipper/TextureDecoder/issues/19
		(value * 15u + 135u) >> 8;
}
