// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2 OR LGPL-3.0-or-later
// You may choose either license when using or modifying this code.

using System.Numerics;
using System.Runtime.CompilerServices;
using Triton.Pixel;

namespace Triton;

// Color Primitive identifier, for fast pixel format comparisons without branches.
public record struct ColorId : IEquatable<ColorId?>, IEquatable<uint>, IEquatable<int>, IEquatable<ushort> {
	public ColorId(ushort value) => Value = value;

	public ColorId(int components, int bits, bool hdr = false, bool signed = false, ChannelLayout layout = ChannelLayout.RedFirst) {
		Value = 0;
		Components = components;
		Bits = bits;
		IsHDR = hdr;
		IsSigned = signed;
		Layout = layout;
	}

	public ushort Value { get; set; }

	// 2 bits for nr of components (0-3)
	// 1 bit for hdr
	// 1 bit for signedness
	// 4 bits for channel layout
	// 8 bits for color bits (0-255), max we reasonably support right now is 128.

	// minimum number of components is 1, so we can save 1 bit by assuming 1 is 0.
	public int Components {
		get => (Value >> 14) + 1;
		set => Value = (ushort) ((Value & ~(0x7u << 14)) | (uint) ((value - 1) << 14));
	}

	public bool IsHDR {
		get => ((Value >> 13) & 1) == 1;
		set => Value = (ushort) ((Value & ~(1u << 13)) | ((value ? 1u : 0u) << 13));
	}

	public bool IsSigned {
		get => ((Value >> 12) & 1) == 1;
		set => Value = (ushort) ((Value & ~(1u << 12)) | ((value ? 1u : 0u) << 12));
	}

	public ChannelLayout Layout {
		get => (ChannelLayout) ((Value >> 8) & 0xf);
		set => Value = (ushort) ((Value & ~(0xf << 8)) | (((ushort) value << 8) & 0xF));
	}

	public int Bits {
		get => Value & 0xFF;
		set => Value = (ushort) ((Value & ~0xFFu) | (byte) value);
	}

	public bool Equals(ColorId other) => other.Value == Value;
	public bool Equals(ColorId? other) => other != null && other.Value.Value == Value;
	public bool Equals(int other) => other == Value;
	public bool Equals(uint other) => other == Value;
	public bool Equals(ushort other) => other == Value;

	public bool Equals<TColor, T>()
		where TColor : unmanaged, IColor<TColor, T>, IColor
		where T : unmanaged, INumberBase<T> => FromPixel<TColor, T>().Equals(this);

	public static ColorId FromPixel<TColor, T>()
		where TColor : unmanaged, IColor<TColor, T>, IColor
		where T : unmanaged, INumberBase<T> {
		var components = Unsafe.SizeOf<TColor>() / Unsafe.SizeOf<T>();
		var bits = Unsafe.SizeOf<T>() << 3;

		if (components > 4) {
			throw new InvalidOperationException("Too many color channels");
		}

		if (bits > 255) {
			throw new InvalidOperationException("Too many bits per color channel");
		}

		var isHDR = typeof(T) == typeof(float) || typeof(T) == typeof(Half);
		var isSigned = typeof(T) == typeof(sbyte) || typeof(T) == typeof(short) || typeof(T) == typeof(int);

		return new ColorId(components, bits, isHDR, isSigned, TColor.ChannelLayout);
	}

	public override int GetHashCode() => Value;
}
