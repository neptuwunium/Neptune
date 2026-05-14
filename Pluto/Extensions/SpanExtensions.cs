using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Pluto.SourceGen.ReverseEndiannessGenerator;

// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Pluto.Extensions;

public static class SpanExtensions {
	extension<T>(Span<T> data) where T : struct {
		public Span<byte> AsBytes() => MemoryMarshal.AsBytes(data);
		public Span<TTo> As<TTo>() where TTo : struct => MemoryMarshal.Cast<T, TTo>(data);
		public Span<T> Clone() => ((ReadOnlySpan<T>) data).Clone();
	}

	extension<T>(Span<T> data) where T : struct, IEquatable<T> {
		public string? ReadString(Encoding encoding, out int length, T terminator = default, int limit = -1) => ((ReadOnlySpan<T>) data).ReadString(encoding, out length, terminator, limit);
		public string? ReadString(Encoding encoding, int limit = -1) => ((ReadOnlySpan<T>) data).ReadString(encoding, out _, default, limit);
	}

	extension<T>(ReadOnlySpan<T> data) where T : struct {
		public ReadOnlySpan<byte> AsBytes() => MemoryMarshal.AsBytes(data);
		public ReadOnlySpan<TTo> As<TTo>() where TTo : struct => MemoryMarshal.Cast<T, TTo>(data);

		public Span<T> Clone() {
			var clone = new T[data.Length];
			data.CopyTo(clone);
			return new Span<T>(clone);
		}
	}

	extension<T>(ReadOnlySpan<T> data) where T : struct, IEquatable<T> {
		public string? ReadString(Encoding encoding, out int length, T terminator = default, int limit = -1) {
			if (limit > -1) {
				data = data[..limit];
			}

			if (data.Length == 0 || data[0].Equals(terminator)) {
				length = 0;
				return default;
			}

			length = data.IndexOf(terminator);
			if (length <= -1) {
				length = data.Length;
			}

			return encoding.GetString(data[..length].AsBytes());
		}

		public string? ReadString(Encoding encoding, int limit = -1) => data.ReadString(encoding, out _, default, limit);
	}

	extension(Span<ushort> data) {
		public void ReverseEndianness() {
			for (var i = 0; i < data.Length; i++) {
				data[i] = BinaryPrimitives.ReverseEndianness(data[i]);
			}
		}
	}

	extension(Span<short> data) {
		public void ReverseEndianness() {
			for (var i = 0; i < data.Length; i++) {
				data[i] = BinaryPrimitives.ReverseEndianness(data[i]);
			}
		}
	}

	extension(Span<uint> data) {
		public void ReverseEndianness() {
			for (var i = 0; i < data.Length; i++) {
				data[i] = BinaryPrimitives.ReverseEndianness(data[i]);
			}
		}
	}

	extension(Span<int> data) {
		public void ReverseEndianness() {
			for (var i = 0; i < data.Length; i++) {
				data[i] = BinaryPrimitives.ReverseEndianness(data[i]);
			}
		}
	}

	extension(Span<ulong> data) {
		public void ReverseEndianness() {
			for (var i = 0; i < data.Length; i++) {
				data[i] = BinaryPrimitives.ReverseEndianness(data[i]);
			}
		}
	}

	extension(Span<long> data) {
		public void ReverseEndianness() {
			for (var i = 0; i < data.Length; i++) {
				data[i] = BinaryPrimitives.ReverseEndianness(data[i]);
			}
		}
	}

	extension(Span<Half> data) {
		public void ReverseEndianness() => MemoryMarshal.Cast<Half, ushort>(data).ReverseEndianness();
	}

	extension(Span<float> data) {
		public void ReverseEndianness() => MemoryMarshal.Cast<float, uint>(data).ReverseEndianness();
	}

	extension(Span<double> data) {
		public void ReverseEndianness() => MemoryMarshal.Cast<double, ulong>(data).ReverseEndianness();
	}

	extension<T>(Span<T> data) where T : IEndianReversible<T> {
		public void ReverseEndianness() {
			for (var i = 0; i < data.Length; i++) {
				data[i] = data[i].ReverseEndianness();
			}
		}
	}
}
