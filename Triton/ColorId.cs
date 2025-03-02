using System.Runtime.CompilerServices;

namespace Triton;

// Color Primitive identifier, for fast pixel format comparisons without branches.
public record struct ColorId : IEquatable<ColorId?>, IEquatable<uint>, IEquatable<int>, IEquatable<ushort> {
	public ushort Value { get; set; }

	// 2 bits for nr of components (0-3)
	// 1 bit for hdr
	// 1 bit for signedness
	// 4 reserved bits
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

	public int Bits {
		get => Value & 0xFF;
		set => Value = (ushort) ((Value & ~0xFFu) | (byte) value);
	}

	public ColorId(ushort value) {
		Value = value;
	}

	public ColorId(int components, int bits, bool hdr = false, bool signed = false) {
		Components = components;
		Bits = bits;
		IsHDR = hdr;
		IsSigned = signed;
	}

	public static ColorId FromPixel<TColor, T>(bool? overrideIsSigned = null) where TColor : struct where T : unmanaged {
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

		if (overrideIsSigned.HasValue) {
			isSigned = overrideIsSigned.Value;
		}

		return new ColorId(components, bits, isHDR, isSigned);
	}

	public bool Equals(ushort other) => other == Value;
	public bool Equals(uint other) => other == Value;
	public bool Equals(int other) => other == Value;
	public bool Equals(ColorId other) => other.Value == Value;
	public bool Equals(ColorId? other) => other != null && other.Value.Value == Value;
	public bool Equals<TColor, T>() where TColor : struct where T : unmanaged => FromPixel<TColor, T>().Equals(this);

	public override int GetHashCode() => Value;
}
