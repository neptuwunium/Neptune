// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Triton.Surface.DirectDraw;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct DDSHeader {
	[InlineArray(11)]
	public struct Reserved1Array : IEquatable<Reserved1Array> {
		public uint Value;

		public override int GetHashCode() {
			var hc = new HashCode();
			hc.AddBytes(MemoryMarshal.AsBytes((ReadOnlySpan<uint>) this));
			return hc.ToHashCode();
		}

		public static bool operator ==(Reserved1Array left, Reserved1Array right) => left.Equals(right);
		public static bool operator !=(Reserved1Array left, Reserved1Array right) => !(left == right);
		public override bool Equals(object? obj) => obj is Reserved1Array other && Equals(other);
		public bool Equals(Reserved1Array other) => ((ReadOnlySpan<uint>) this).SequenceEqual(other);
	}

	public uint Magic { get; set; }
	public int Size { get; set; }
	public DDSFlags Flags { get; set; }
	public int Height { get; set; }
	public int Width { get; set; }
	public int PitchOrLinearSize { get; set; }
	public int Depth { get; set; }
	public int MipMapCount { get; set; }
	public Reserved1Array Reserved1 { get; set; }
	public DDSPixelFormat PixelFormat { get; set; }
	public DDSCaps1 Caps1 { get; set; }
	public DDSCaps2 Caps2 { get; set; }
	public DDSCaps3 Caps3 { get; set; }
	public DDSCaps4 Caps4 { get; set; }
	public uint Reserved2 { get; set; }
}
