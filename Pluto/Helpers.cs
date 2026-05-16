// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pluto;

public static class Helpers {
	public static void ResetCulture() =>
		Thread.CurrentThread.CurrentCulture =
			Thread.CurrentThread.CurrentUICulture =
				CultureInfo.CurrentCulture =
					CultureInfo.CurrentUICulture =
						CultureInfo.DefaultThreadCurrentCulture =
							CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

	public static T ReverseStruct<T, TInner>(T value) where T : unmanaged {
		var elementSize = Unsafe.SizeOf<TInner>();
		if (elementSize <= 1) {
			return value;
		}

		var reversed = value;
		var bytes = MemoryMarshal.AsBytes(new Span<T>(ref reversed));
		if (bytes.Length == elementSize) {
			bytes.Reverse();
			return reversed;
		}

		if (bytes.Length % elementSize != 0) {
			throw new InvalidOperationException("struct is not a multiple!");
		}

		for (var i = 0; i < bytes.Length; i += elementSize) {
			bytes.Slice(i, elementSize).Reverse();
		}

		return reversed;
	}
}
