// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pluto.IO.Binary;
using Triton.Pixel.Formats;
using Triton.Surface.Compression;
using Triton.Surface.DirectDraw;

namespace Triton.Surface;

public class DDS : IDisposable {
	public DDS(IRentedArray<byte> buffer, DDSHeader header, DDSHeader10 header10, bool leaveOpen = false) {
		Buffer = buffer;
		LeaveOpen = leaveOpen;
		Header = header;
		Header10 = header10;
		DataStart = 0;
		SetupProperties();
	}

	public DDS(IRentedArray<byte> buffer, bool leaveOpen = false) {
		Buffer = buffer;
		LeaveOpen = leaveOpen;

		DataStart = Unsafe.SizeOf<DDSHeader>();
		if (buffer.Length < DataStart) {
			throw new IndexOutOfRangeException();
		}

		var span = buffer.Span;
		Header = MemoryMarshal.Read<DDSHeader>(span);

		if (Header.PixelFormat.FourCC == D3DFORMAT.DX10) {
			var size = Unsafe.SizeOf<DDSHeader10>();
			if (buffer.Length < DataStart + size) {
				throw new IndexOutOfRangeException();
			}

			Header10 = MemoryMarshal.Read<DDSHeader10>(span[DataStart..]);
			DataStart += size;
		}

		SetupProperties();
	}

	private void SetupProperties() {
		if (Header.PixelFormat.FourCC == D3DFORMAT.DX10) {
			Format = Header10.DXGIFormat;
			ArrayCount = Header10.ArraySize * ((Header10.MiscFlag & D3D11ResourceMisc.TextureCube) != 0 ? 6 : 1);
		} else {
			Format = Header.PixelFormat.DXGI;
			ArrayCount = (Header.Caps2 & DDSCaps2.Cubemap) != 0 ? 6 : 1;
		}

		Mips = (Header.Caps1 & DDSCaps1.Mipmap) != 0 ? Header.MipMapCount : 1;
		OneSurface = (int) CalculateSurfaceSize(Header.Width, Header.Height, Format, Mips, out var largestMip);
		LargeSurface = (int) largestMip;
	}

	~DDS() => Dispose(false);

	public bool LeaveOpen { get; }
	public IRentedArray<byte> Buffer { get; }
	public int DataStart { get; }
	public DDSHeader Header { get; set; }
	public DDSHeader10 Header10 { get; set; }
	public DXGIFormat Format { get; set; }
	public int ArrayCount { get; set; }
	public int LargeSurface { get; set; }
	public int OneSurface { get; set; }
	public int Mips { get; set; }

	public IImageBuffer? GetSurface(int surfaceIndex, bool decompress = true) {
		if (Format == DXGIFormat.UNKNOWN) {
			return null;
		}

		if (Header10.ResourceDimension is not (D3D11ResourceDimension.Texture2D or D3D11ResourceDimension.Unknown)) {
			throw new NotSupportedException();
		}

		var offset = DataStart + OneSurface * surfaceIndex;
		var pixelData = new UnownedRentedArray<byte>(Buffer, offset, OneSurface);

		if (Format.IsCompressed && decompress) {
			// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
			switch (Format) {
				case DXGIFormat.BC1_UNORM:
				case DXGIFormat.BC1_UNORM_SRGB: {
					var decompressed = new RentedArray<byte>(Header.Width * Header.Height * 4);
					BCDec.DecompressBC1(pixelData.Memory, decompressed.Memory, Header.Width, Header.Height);
					return new ImageBuffer<ColorRGBA<byte>, byte>(decompressed, Header.Width, Header.Height);
				}
				case DXGIFormat.BC2_UNORM:
				case DXGIFormat.BC2_UNORM_SRGB: {
					var decompressed = new RentedArray<byte>(Header.Width * Header.Height * 4);
					BCDec.DecompressBC2(pixelData.Memory, decompressed.Memory, Header.Width, Header.Height);
					return new ImageBuffer<ColorRGBA<byte>, byte>(decompressed, Header.Width, Header.Height);
				}
				case DXGIFormat.BC3_UNORM:
				case DXGIFormat.BC3_UNORM_SRGB: {
					var decompressed = new RentedArray<byte>(Header.Width * Header.Height * 4);
					BCDec.DecompressBC3(pixelData.Memory, decompressed.Memory, Header.Width, Header.Height);
					return new ImageBuffer<ColorRGBA<byte>, byte>(decompressed, Header.Width, Header.Height);
				}
				case DXGIFormat.BC4_UNORM:
				case DXGIFormat.BC4_SNORM: {
					var decompressed = new RentedArray<byte>(Header.Width * Header.Height);
					BCDec.DecompressBC4(pixelData.Memory, decompressed.Memory, Header.Width, Header.Height, Format == DXGIFormat.BC4_SNORM);
					return new ImageBuffer<ColorR<byte>, byte>(decompressed, Header.Width, Header.Height);
				}
				case DXGIFormat.BC5_UNORM:
				case DXGIFormat.BC5_SNORM: {
					var decompressed = new RentedArray<byte>(Header.Width * Header.Height * 2);
					BCDec.DecompressBC5(pixelData.Memory, decompressed.Memory, Header.Width, Header.Height, Format == DXGIFormat.BC5_SNORM);
					return new ImageBuffer<ColorRG<byte>, byte>(decompressed, Header.Width, Header.Height);
				}
				case DXGIFormat.BC6H_SF16:
				case DXGIFormat.BC6H_UF16: {
					var decompressed = new RentedArray<byte>(Header.Width * Header.Height * 3 * 2);
					BCDec.DecompressBC6H(pixelData.Memory, decompressed.Memory, Header.Width, Header.Height, Format == DXGIFormat.BC6H_UF16);
					return new ImageBuffer<ColorRGB<Half>, Half>(decompressed, Header.Width, Header.Height);
				}
				case DXGIFormat.BC7_UNORM:
				case DXGIFormat.BC7_UNORM_SRGB: {
					var decompressed = new RentedArray<byte>(Header.Width * Header.Height * 4);
					BCDec.DecompressBC7(pixelData.Memory, decompressed.Memory, Header.Width, Header.Height);
					return new ImageBuffer<ColorRGBA<byte>, byte>(decompressed, Header.Width, Header.Height);
				}
				default:
					throw new UnreachableException();
			}
		}

		switch (Format) {
			case DXGIFormat.BC1_UNORM:
			case DXGIFormat.BC1_UNORM_SRGB:
				return new BlockCompressedImageBuffer(pixelData, Header.Width, Header.Height, ImageCompression.BC1);
			case DXGIFormat.BC2_UNORM:
			case DXGIFormat.BC2_UNORM_SRGB:
				return new BlockCompressedImageBuffer(pixelData, Header.Width, Header.Height, ImageCompression.BC2);
			case DXGIFormat.BC3_UNORM:
			case DXGIFormat.BC3_UNORM_SRGB:
				return new BlockCompressedImageBuffer(pixelData, Header.Width, Header.Height, ImageCompression.BC3);
			case DXGIFormat.BC4_UNORM:
				return new BlockCompressedImageBuffer(pixelData, Header.Width, Header.Height, ImageCompression.BC4U);
			case DXGIFormat.BC4_SNORM:
				return new BlockCompressedImageBuffer(pixelData, Header.Width, Header.Height, ImageCompression.BC4S);
			case DXGIFormat.BC5_UNORM:
				return new BlockCompressedImageBuffer(pixelData, Header.Width, Header.Height, ImageCompression.BC5U);
			case DXGIFormat.BC5_SNORM:
				return new BlockCompressedImageBuffer(pixelData, Header.Width, Header.Height, ImageCompression.BC5S);
			case DXGIFormat.BC6H_SF16:
				return new BlockCompressedImageBuffer(pixelData, Header.Width, Header.Height, ImageCompression.BC6S);
			case DXGIFormat.BC6H_UF16:
				return new BlockCompressedImageBuffer(pixelData, Header.Width, Header.Height, ImageCompression.BC6U);
			case DXGIFormat.BC7_UNORM:
			case DXGIFormat.BC7_UNORM_SRGB:
				return new BlockCompressedImageBuffer(pixelData, Header.Width, Header.Height, ImageCompression.BC7);
			case DXGIFormat.R32G32B32A32_FLOAT:
				return new ImageBuffer<ColorRGBA<float>, float>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R32G32B32A32_UINT:
				return new ImageBuffer<ColorRGBA<uint>, uint>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R32G32B32A32_SINT:
				return new ImageBuffer<ColorRGBA<int>, int>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R32G32B32_FLOAT:
				return new ImageBuffer<ColorRGB<float>, float>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R32G32B32_UINT:
				return new ImageBuffer<ColorRGB<uint>, uint>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R32G32B32_SINT:
				return new ImageBuffer<ColorRGB<int>, int>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R16G16B16A16_FLOAT:
				return new ImageBuffer<ColorRGBA<Half>, Half>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R16G16B16A16_UNORM:
			case DXGIFormat.R16G16B16A16_UINT:
				return new ImageBuffer<ColorRGBA<ushort>, ushort>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R16G16B16A16_SNORM:
			case DXGIFormat.R16G16B16A16_SINT:
				return new ImageBuffer<ColorRGBA<short>, short>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R32G32_FLOAT:
				return new ImageBuffer<ColorRG<float>, float>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R32G32_UINT:
				return new ImageBuffer<ColorRG<uint>, uint>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R32G32_SINT:
				return new ImageBuffer<ColorRG<int>, int>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R10G10B10A2_UNORM:
			case DXGIFormat.R10G10B10A2_UINT:
				return new ImageBuffer<ColorR10G10B10A2, float>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R11G11B10_FLOAT:
				return new ImageBuffer<ColorR11G11B10, float>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R8G8B8A8_UNORM:
			case DXGIFormat.R8G8B8A8_UNORM_SRGB:
			case DXGIFormat.R8G8B8A8_UINT:
				return new ImageBuffer<ColorRGBA<byte>, byte>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R8G8B8A8_SNORM:
			case DXGIFormat.R8G8B8A8_SINT:
				return new ImageBuffer<ColorRGBA<sbyte>, sbyte>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R16G16_FLOAT:
				return new ImageBuffer<ColorRG<Half>, Half>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R16G16_UNORM:
			case DXGIFormat.R16G16_UINT:
				return new ImageBuffer<ColorRG<ushort>, ushort>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R16G16_SNORM:
			case DXGIFormat.R16G16_SINT:
				return new ImageBuffer<ColorRG<short>, short>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R32_FLOAT:
				return new ImageBuffer<ColorR<float>, float>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R32_UINT:
				return new ImageBuffer<ColorR<uint>, uint>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R32_SINT:
				return new ImageBuffer<ColorR<int>, int>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R8G8_UNORM:
			case DXGIFormat.R8G8_UINT:
				return new ImageBuffer<ColorRG<uint>, uint>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R8G8_SNORM:
			case DXGIFormat.R8G8_SINT:
				return new ImageBuffer<ColorRG<int>, int>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R16_FLOAT:
				return new ImageBuffer<ColorR<Half>, Half>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R16_UNORM:
			case DXGIFormat.R16_UINT:
				return new ImageBuffer<ColorR<ushort>, ushort>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R16_SNORM:
			case DXGIFormat.R16_SINT:
				return new ImageBuffer<ColorR<short>, short>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R8_UNORM:
			case DXGIFormat.R8_UINT:
				return new ImageBuffer<ColorR<byte>, byte>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R8_SNORM:
			case DXGIFormat.R8_SINT:
				return new ImageBuffer<ColorR<sbyte>, sbyte>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.A8_UNORM:
				return new ImageBuffer<ColorA<byte>, byte>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.R9G9B9E5_SHAREDEXP:
				return new ImageBuffer<ColorRGB9e5, double>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.B5G6R5_UNORM:
				return new ImageBuffer<ColorB5G6R5, float>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.B5G5R5A1_UNORM:
				return new ImageBuffer<ColorB5G5R5A1, float>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.B8G8R8A8_UNORM:
			case DXGIFormat.B8G8R8A8_UNORM_SRGB:
				return new ImageBuffer<ColorBGRA<byte>, byte>(pixelData, Header.Width, Header.Height);
			case DXGIFormat.B8G8R8X8_UNORM:
			case DXGIFormat.B8G8R8X8_UNORM_SRGB:
				return new ImageBuffer<ColorBGR<byte>, byte>(pixelData, Header.Width, Header.Height);
			default: throw new NotSupportedException($"format {Format} is not supported");
		}
	}

	public static uint CalculateSurfaceSize(int width, int height, DXGIFormat format, int numMips, out uint largestMip) {
		var (bpb, ppb) = format.PitchFactor;
		return CalculateSurfaceSize(width, height, ppb, bpb, numMips, out largestMip);
	}

	public static uint CalculateSurfaceSize(uint pitch, DXGIFormat format, int numMips) {
		var (bpb, ppb) = format.PitchFactor;
		return CalculateSurfaceSize(pitch, ppb, bpb, numMips);
	}

	public static uint CalculateSurfaceSize(int width, int height, uint pixelsPerBlock, uint bitsPerBlock, int numMips, out uint largestMip) {
		if (pixelsPerBlock == 0 || bitsPerBlock == 0) {
			largestMip = 0;
			return 0;
		}

		var mask = ((uint) width * (uint) height / pixelsPerBlock * bitsPerBlock) >> 3;
		largestMip = mask;
		return CalculateSurfaceSize(mask, pixelsPerBlock, bitsPerBlock, numMips);
	}

	public static uint CalculateSurfaceSize(uint pitch, uint pixelsPerBlock, uint bitsPerBlock, int numMips) {
		if (pixelsPerBlock == 0 || bitsPerBlock == 0) {
			return 0;
		}

		// this will always work as long as width and height are powers of 2
		var oneSurface = 0u;
		for (var i = 0; i < numMips; ++i) {
			oneSurface ^= pitch; // maybe use += instead of ^= for non-power-of-2?
			pitch >>= 2;
		}
		return oneSurface;
	}

	public static uint CalculateSurfaceSize(int width, int height, DXGIFormat format, out uint largestMip) => CalculateSurfaceSize(width, height, format, 1, out largestMip);
	public static uint CalculateSurfaceSize(int width, int height, uint pixelsPerBlock, uint bitsPerBlock, out uint largestMip) => CalculateSurfaceSize(width, height, pixelsPerBlock, bitsPerBlock, 1, out largestMip);

	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			Buffer.Dispose();
		}
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
