// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.InteropServices;

namespace Triton.Surface.DirectDraw;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public record struct DDSPixelFormat {
	public int Size { get; set; }
	public DDSPixelFlags Flags { get; set; }
	public D3DFORMAT FourCC { get; set; }
	public int RGBBitCount { get; set; }
	public uint RBitMask { get; set; }
	public uint GBitMask { get; set; }
	public uint BBitMask { get; set; }
	public uint ABitMask { get; set; }

	public DXGIFormat DXGI {
		get {
			if ((Flags & DDSPixelFlags.FourCC) != 0) {
				return FourCC.DXGI;
			}

			var r = RBitMask;
			var g = GBitMask;
			var b = BBitMask;
			var a = (Flags & DDSPixelFlags.AlphaPixels) != 0 ? ABitMask : 0;

			var rBits = BitOperations.PopCount(r);
			var gBits = BitOperations.PopCount(g);
			var bBits = BitOperations.PopCount(b);
			var aBits = BitOperations.PopCount(a);
			var bFirst = b > r;
			var hasAlpha = a > 0;

			if (rBits + gBits + bBits + aBits == 0) {
				return DXGIFormat.UNKNOWN;
			}

			switch (rBits) {
				case 8 when gBits == 8 && bBits == 8 && aBits == 8:
					return bFirst ? DXGIFormat.R8G8B8A8_UNORM : DXGIFormat.B8G8R8A8_UNORM;
				case 8 when gBits == 8 && bBits == 8:
					return DXGIFormat.B8G8R8X8_UNORM;
				case 5 when gBits == 6 && bBits == 5:
					return DXGIFormat.B5G6R5_UNORM;
				case 5 when gBits == 5 && bBits == 5:
					return DXGIFormat.B5G5R5A1_UNORM;
				case 4 when gBits == 4 && bBits == 4:
					return DXGIFormat.B4G4R4A4_UNORM;
				case 10 when gBits == 10 && bBits == 10 && aBits == 2:
					return DXGIFormat.R10G10B10A2_UNORM;
				case 16 when gBits == 16 && aBits == 16:
					return DXGIFormat.R16G16B16A16_UNORM;
				case 16 when gBits == 16 && aBits == 0:
					return DXGIFormat.R16G16_UNORM;
				case 8 when gBits == 8 && bBits == 0 && aBits == 0:
					return DXGIFormat.R8G8_UNORM;
				case 8 when gBits == 0 && bBits == 0:
					return hasAlpha && aBits == 8 ? DXGIFormat.R8G8_UNORM : DXGIFormat.R8_UNORM;
				case 16 when gBits == 0 && bBits == 0 && aBits == 0:
					return DXGIFormat.R16_UNORM;
				case 0 when gBits == 0 && bBits == 0 && aBits == 8:
					return DXGIFormat.A8_UNORM;
				case 0 when (Flags & DDSPixelFlags.AlphaPixels) != 0:
					switch (BitOperations.PopCount(ABitMask)) {
						case 1:
							return DXGIFormat.R1_UNORM;
						case 8:
							return DXGIFormat.A8_UNORM;
						case 16:
							return DXGIFormat.R16_UNORM;
						case 32:
							return DXGIFormat.R32_UINT;
					}
					break;
			}

			return DXGIFormat.UNKNOWN;
		}
	}
}
