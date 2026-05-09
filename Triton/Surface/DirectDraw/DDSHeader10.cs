// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;

namespace Triton.Surface.DirectDraw;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct DDSHeader10 {
	public DXGIFormat DXGIFormat { get; set; }
	public D3D11ResourceDimension ResourceDimension { get; set; }
	public D3D11ResourceMisc MiscFlag { get; set; }
	public int ArraySize { get; set; }
	public uint AlphaMiscFlags { get; set; }
	public DDSAlphaMode AlphaMode => (DDSAlphaMode) (AlphaMiscFlags & 0b111);
	public uint MiscFlags2 => AlphaMiscFlags >> 3;
}
