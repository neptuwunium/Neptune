// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using Pluto.IO.Binary;
using Pluto.Maths;
using Triton.Pixel;
using Triton.Pixel.Formats;

namespace Triton;

public sealed class BlockCompressedImageBuffer : IImageBuffer {
	public BlockCompressedImageBuffer(IRentedArray<byte> buffer, Point<int> size, ImageCompression compression) : this(buffer, size.X, size.Y, compression) { }
	public BlockCompressedImageBuffer(IRentedArray<byte> buffer, int width, int height, ImageCompression compression) {
		Data = buffer;
		Width = width;
		Height = height;
		Size = new Point<int>(width, height);
		Compression = compression;
		
		switch (compression) {
			case ImageCompression.Linear:
				throw new NotSupportedException();
			case ImageCompression.BC1:
				ColorId = ColorId.FromPixel<ColorRGBA<byte>, byte>();
				Stride = 8;
				break;
			case ImageCompression.BC2:
			case ImageCompression.BC3:
				ColorId = ColorId.FromPixel<ColorRGBA<byte>, byte>();
				Stride = 16;
				break;
			case ImageCompression.BC4S: 
				ColorId = ColorId.FromPixel<ColorR<sbyte>, sbyte>();
				Stride = 8;
				break;
			case ImageCompression.BC4U: 
				ColorId = ColorId.FromPixel<ColorR<byte>, byte>();
				Stride = 8;
				break;
			case ImageCompression.BC5S: 
				ColorId = ColorId.FromPixel<ColorRG<sbyte>, sbyte>();
				Stride = 16;
				break;
			case ImageCompression.BC5U: 
				ColorId = ColorId.FromPixel<ColorRG<byte>, byte>();
				Stride = 16;
				break;
			case ImageCompression.BC6S: 
				ColorId = ColorId.FromPixel<ColorRGB<short>, short>();
				Stride = 16;
				break;
			case ImageCompression.BC6U: 
				ColorId = ColorId.FromPixel<ColorRGB<ushort>, ushort>();
				Stride = 16;
				break;
			case ImageCompression.BC7: 
				ColorId = ColorId.FromPixel<ColorRGBA<byte>, byte>();
				Stride = 16;
				break;
			default: throw new ArgumentOutOfRangeException(nameof(compression), compression, null);
		}
		
	}
	public IRentedArray<byte> Data { get; }
	public int Width { get; }
	public int Height { get; }
	public Point<int> Size { get; }
	public int Stride { get; }
	public ColorId ColorId { get; }
	public ImageCompression Compression { get; }

	public ImageBuffer<TNewColor, TNew> Cast<TNewColor, TNew>() where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> => throw new NotSupportedException();
	public IImageBuffer Cast<TNew>() where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> => throw new NotSupportedException();
	public IImageBuffer Cast(int components) => throw new NotSupportedException();
	public IImageBuffer CreateSubImage(int width, int height) => throw new NotSupportedException();
	public IImageBuffer CreateSubImage(Point<int> size) => throw new NotSupportedException();
	public void PremultiplyAlpha() => throw new NotSupportedException();
	public void StraightAlpha() => throw new NotSupportedException();
	public IColor Sample(float x, float y, SamplingOperation operation = SamplingOperation.Bilinear, SamplingWrap wrap = SamplingWrap.Repeat) => throw new NotSupportedException();
	public IColor Sample(Point<float> target, SamplingOperation operation = SamplingOperation.Bilinear, SamplingWrap wrap = SamplingWrap.Repeat) => throw new NotSupportedException();
	public IImageBuffer Resize(int x, int y, SamplingOperation operation = SamplingOperation.Bilinear) => throw new NotSupportedException();
	public IImageBuffer Resize(Point<int> target, SamplingOperation operation = SamplingOperation.Bilinear) => throw new NotSupportedException();
	public IImageBuffer Rotate(float degrees, float? x = null, float? y = null, SamplingOperation operation = SamplingOperation.Bilinear) => throw new NotSupportedException();
	public IImageBuffer Rotate(float degrees, Point<float>? pivot = null, SamplingOperation operation = SamplingOperation.Bilinear) => throw new NotSupportedException();
	public void Alpha(float alpha, int x, int y, PixelOperation operation = PixelOperation.Copy) => throw new NotSupportedException();
	public void Alpha(float alpha, Point<int> target, PixelOperation operation = PixelOperation.Copy) => throw new NotSupportedException();
	public void Clear(int x, int y, PixelOperation operation = PixelOperation.Copy) => throw new NotSupportedException();
	public void Clear(Point<int> target, PixelOperation operation = PixelOperation.Copy) => throw new NotSupportedException();
	public void Draw(IColor pixel, int x, int y, PixelOperation operation = PixelOperation.Copy) => throw new NotSupportedException();
	public void Draw(IColor pixel, Point<int> target, PixelOperation operation = PixelOperation.Copy) => throw new NotSupportedException();
	public void Draw(IImageBuffer image, int x, int y, PixelOperation operation = PixelOperation.Copy) => throw new NotSupportedException();
	public void Draw(IImageBuffer image, Point<int> target, PixelOperation operation = PixelOperation.Copy) => throw new NotSupportedException();
	public void Draw(IImageBuffer image, Point<int> target, Rect<int> crop, PixelOperation operation = PixelOperation.Copy) => throw new NotSupportedException();
	public void Flip() => throw new NotSupportedException();
	public void Flop() => throw new NotSupportedException();
	public void Clear() => throw new NotSupportedException();
	public void Clear(Rect<int> area) => throw new NotSupportedException();
	public void Clear<TColor, T>(TColor color, PixelOperation operation = PixelOperation.Copy) where TColor : unmanaged, IColor<TColor, T>, IColor where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => throw new NotSupportedException();
	public void Clear<TColor, T>(TColor color, int x, int y, PixelOperation operation = PixelOperation.Copy) where TColor : unmanaged, IColor<TColor, T>, IColor where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => throw new NotSupportedException();
	public void Clear<TColor, T>(TColor color, Point<int> target, PixelOperation operation = PixelOperation.Copy) where TColor : unmanaged, IColor<TColor, T>, IColor where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => throw new NotSupportedException();
	public void Clear<TColor, T>(TColor color, Rect<int> target, PixelOperation operation = PixelOperation.Copy) where TColor : unmanaged, IColor<TColor, T>, IColor where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => throw new NotSupportedException();

	public IImageBuffer Clone() {
		var pitch = (Width >> 2) * (Height >> 2) * Stride;
		var buffer = new RentedArray<byte>(pitch);
		Data.Span[..pitch].CopyTo(buffer.Span);
		var image = new BlockCompressedImageBuffer(buffer, Size, Compression);
		return image;
	}

	public void Dispose() => Data.Dispose();
}
