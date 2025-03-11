// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;

namespace Triton;

public record struct Rect<T>(Point<T> TopLeft, Point<T> WidthHeight) : IMinMaxValue<Rect<T>> where T : INumber<T>, IMinMaxValue<T> {
	public static Rect<T> MaxValue { get; } = new(Point<T>.MaxValue, Point<T>.MaxValue);
	public static Rect<T> MinValue { get; } = new(Point<T>.MinValue, Point<T>.MinValue);
	public static Rect<T> Zero { get; } = new(Point<T>.Zero, Point<T>.Zero);
}
