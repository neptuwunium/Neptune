// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.Intrinsics;

namespace Charon.Encryption;

public static class SIMDUtils {
	public static readonly Vector128<byte> ReverseBytes = Vector128.Create(new byte[] { 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 });
	public static readonly Vector128<byte> ReverseEndianness32 = Vector128.Create(new byte[] { 3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12 });
}
