// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton;

public readonly record struct CubemapOrder() {
	public static CubemapOrder DXGIOrder { get; } = new();
	public int PositiveX { get; init; } = 0;
	public int NegativeX { get; init; } = 1;
	public int PositiveY { get; init; } = 2;
	public int NegativeY { get; init; } = 3;
	public int PositiveZ { get; init; } = 4;
	public int NegativeZ { get; init; } = 5;

	public int this[int index] =>
		index switch {
			0 => PositiveX,
			1 => NegativeX,
			2 => PositiveY,
			3 => NegativeY,
			4 => PositiveZ,
			5 => NegativeZ,
			_ => throw new IndexOutOfRangeException(),
		};
}
