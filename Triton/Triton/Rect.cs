// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2 OR LGPL-3.0-or-later
// You may choose either license when using or modifying this code.

using System.Numerics;

namespace Triton;

public readonly record struct Rect<T>(Point<T> TopLeft, Point<T> WidthHeight) : IMinMaxValue<Rect<T>> where T : INumber<T>, IMinMaxValue<T> {
	public Rect(T topLeft, T widthHeight) : this(new Point<T>(topLeft), new Point<T>(widthHeight)) { }
	public Rect(T top, T left, T width, T height) : this(new Point<T>(top, left), new Point<T>(width, height)) { }
	public Rect(Point<T> topLeft, T widthHeight) : this(topLeft, new Point<T>(widthHeight)) { }
	public Rect(T topLeft, Point<T> widthHeight) : this(new Point<T>(topLeft), widthHeight) { }

	public static Rect<T> Zero { get; } = new(Point<T>.Zero, Point<T>.Zero);
	public static Rect<T> MaxValue { get; } = new(Point<T>.MaxValue, Point<T>.MaxValue);
	public static Rect<T> MinValue { get; } = new(Point<T>.MinValue, Point<T>.MinValue);
}
