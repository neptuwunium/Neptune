// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Random;

[StructLayout(LayoutKind.Explicit)]
public ref struct XorShift128 {
	[FieldOffset(0x0)]
	public uint A;

	[FieldOffset(0x4)]
	public uint B;

	[FieldOffset(0x8)]
	public uint C;

	[FieldOffset(0xC)]
	public uint D;

	[FieldOffset(0x10)]
	public uint Seed;

	public XorShift128(uint seed, uint prime = 0x6C078965) : this() {
		Seed = seed;
		A = Seed;
		B = prime * A + 1;
		C = prime * B + 1;
		D = prime * C + 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public uint Next() {
		var t = A ^ (A << 11);
		A = B;
		B = C;
		C = D;
		return D = D ^ (D >> 19) ^ t ^ (t >> 8);
	}
}
