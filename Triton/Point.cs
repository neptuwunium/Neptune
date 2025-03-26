// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2 OR LGPL-3.0-or-later
// You may choose either license when using or modifying this code.

using System.Numerics;

namespace Triton;

public readonly record struct Point<T>(T X, T Y) :
	IDivisionOperators<Point<T>, Point<T>, Point<T>>,
	IDivisionOperators<Point<T>, T, Point<T>>,
	IMultiplyOperators<Point<T>, Point<T>, Point<T>>,
	IMultiplyOperators<Point<T>, T, Point<T>>,
	IAdditionOperators<Point<T>, Point<T>, Point<T>>,
	IAdditionOperators<Point<T>, T, Point<T>>,
	ISubtractionOperators<Point<T>, Point<T>, Point<T>>,
	ISubtractionOperators<Point<T>, T, Point<T>>,
	IUnaryNegationOperators<Point<T>, Point<T>>,
	IUnaryPlusOperators<Point<T>, Point<T>>,
	IEqualityOperators<Point<T>, Point<T>, bool>,
	IIncrementOperators<Point<T>>,
	IDecrementOperators<Point<T>>,
	IMinMaxValue<Point<T>>
	where T : INumber<T>, IMinMaxValue<T> {
	public Point(T value) : this(value, value) { }

	public static Point<T> Zero { get; } = new(T.Zero);
	public static Point<T> operator +(Point<T> left, Point<T> right) => new(left.X + right.X, left.Y + right.Y);
	public static Point<T> operator +(Point<T> left, T right) => new(left.X + right, left.Y + right);
	public static Point<T> operator --(Point<T> value) => new(value.X - T.One, value.Y - T.One);
	public static Point<T> operator /(Point<T> left, Point<T> right) => new(left.X / right.X, left.Y / right.Y);
	public static Point<T> operator /(Point<T> left, T right) => new(left.X / right, left.Y / right);
	public static Point<T> operator ++(Point<T> value) => new(value.X + T.One, value.Y + T.One);
	public static Point<T> MaxValue { get; } = new(T.MaxValue, T.MaxValue);
	public static Point<T> MinValue { get; } = new(T.MinValue, T.MinValue);
	public static Point<T> operator *(Point<T> left, Point<T> right) => new(left.X * right.X, left.Y * right.Y);
	public static Point<T> operator *(Point<T> left, T right) => new(left.X * right, left.Y * right);
	public static Point<T> operator -(Point<T> left, Point<T> right) => new(left.X - right.X, left.Y - right.Y);
	public static Point<T> operator -(Point<T> left, T right) => new(left.X - right, left.Y - right);
	static Point<T> IUnaryNegationOperators<Point<T>, Point<T>>.operator -(Point<T> value) => new(-value.X, -value.Y);
	static Point<T> IUnaryPlusOperators<Point<T>, Point<T>>.operator +(Point<T> value) => new(+value.X, +value.Y);
	public static Point<T> operator /(T left, Point<T> right) => new(left / right.X, left / right.Y);
	public Point<TOther> As<TOther>() where TOther : INumber<TOther>, IMinMaxValue<TOther> => CastTruncating<TOther>();
	public Point<TOther> CastChecked<TOther>() where TOther : INumber<TOther>, IMinMaxValue<TOther> => new(TOther.CreateChecked(X), TOther.CreateChecked(Y));
	public Point<TOther> CastSaturating<TOther>() where TOther : INumber<TOther>, IMinMaxValue<TOther> => new(TOther.CreateSaturating(X), TOther.CreateSaturating(Y));
	public Point<TOther> CastTruncating<TOther>() where TOther : INumber<TOther>, IMinMaxValue<TOther> => new(TOther.CreateTruncating(X), TOther.CreateTruncating(Y));
}
