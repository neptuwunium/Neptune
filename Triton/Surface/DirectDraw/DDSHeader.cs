// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Triton.Surface.DirectDraw;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct DDSHeader {
	[InlineArray(11)]
	internal struct Reserved1Array {
		public uint Value;
	}

	public uint Magic { get; set; }
	public int Size { get; set; }
	public DDSFlags Flags { get; set; }
	public int Height { get; set; }
	public int Width { get; set; }
	public int PitchOrLinearSize { get; set; }
	public int Depth { get; set; }
	public int MipMapCount { get; set; }
	internal Reserved1Array Reserved1 { get; set; }
	public DDSPixelFormat PixelFormat { get; set; }
	public DDSCaps1 Caps1 { get; set; }
	public DDSCaps2 Caps2 { get; set; }
	public DDSCaps3 Caps3 { get; set; }
	public DDSCaps4 Caps4 { get; set; }
	internal int Reserved2 { get; set; }
}
