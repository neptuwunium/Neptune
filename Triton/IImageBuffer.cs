// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2 OR LGPL-3.0-or-later
// You may choose either license when using or modifying this code.

using System.Buffers;
using System.Numerics;
using Triton.Pixel;
using Triton.Pixel.Formats;

namespace Triton;

public interface IImageBuffer : IDisposable {
	public IMemoryOwner<byte> Data { get; }
	public int Width { get; }
	public int Height { get; }
	public Point<int> Size { get; }
	public int Stride { get; }
	public ColorId ColorId { get; }
	public bool IsCanonized { get; }

	public IImageBuffer Cast<TNewColor, TNew>()
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew>;

	public IImageBuffer Cast<TNew>()
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew>;

	public IImageBuffer Cast(int components);

	public IImageBuffer CreateSubImage(int width, int height);

	public IImageBuffer CreateSubImage(Point<int> size);

	public void PremultiplyAlpha();
	public void StraightAlpha();

	public IColor Sample(float x, float y, SamplingOperation operation = SamplingOperation.Bilinear, SamplingWrap wrap = SamplingWrap.Repeat);
	public IColor Sample(Point<float> target, SamplingOperation operation = SamplingOperation.Bilinear, SamplingWrap wrap = SamplingWrap.Repeat);

	public IImageBuffer Resize(int x, int y, SamplingOperation operation = SamplingOperation.Bilinear);
	public IImageBuffer Resize(Point<int> target, SamplingOperation operation = SamplingOperation.Bilinear);

	public IImageBuffer Rotate(float degrees, float? x = null, float? y = null, SamplingOperation operation = SamplingOperation.Bilinear);
	public IImageBuffer Rotate(float degrees, Point<float>? pivot = null, SamplingOperation operation = SamplingOperation.Bilinear);

	public void Draw(IColor pixel, int x, int y, PixelOperation operation = PixelOperation.Copy);
	public void Draw(IColor pixel, Point<int> target, PixelOperation operation = PixelOperation.Copy);
	public void Draw(IImageBuffer image, int x, int y, PixelOperation operation = PixelOperation.Copy);
	public void Draw(IImageBuffer image, Point<int> target, PixelOperation operation = PixelOperation.Copy);
	public void Draw(IImageBuffer image, Point<int> target, Rect<int> crop, PixelOperation operation = PixelOperation.Copy);

	public void Flip();
	public void Flop();
	
	public void Clear();
	public void Clear(Rect<int> area);
	public IImageBuffer Clone();

	public void Clear<TColor, T>(TColor color, PixelOperation operation = PixelOperation.Copy)
		where TColor : unmanaged, IColor<TColor, T>, IColor
		where T : unmanaged, INumberBase<T>, IMinMaxValue<T>;

	public void Clear<TColor, T>(TColor color, int x, int y, PixelOperation operation = PixelOperation.Copy)
		where TColor : unmanaged, IColor<TColor, T>, IColor
		where T : unmanaged, INumberBase<T>, IMinMaxValue<T>;

	public void Clear<TColor, T>(TColor color, Point<int> target, PixelOperation operation = PixelOperation.Copy)
		where TColor : unmanaged, IColor<TColor, T>, IColor
		where T : unmanaged, INumberBase<T>, IMinMaxValue<T>;

	public void Clear<TColor, T>(TColor color, Rect<int> target, PixelOperation operation = PixelOperation.Copy)
		where TColor : unmanaged, IColor<TColor, T>, IColor
		where T : unmanaged, INumberBase<T>, IMinMaxValue<T>;

	static IImageBuffer Create(int width, int height, int bitDepth, int samples, bool isFloat, bool isSigned) =>
		bitDepth switch {
			8 when isSigned => Create<sbyte>(width, height, samples),
			8 => Create<byte>(width, height, samples),
			16 when isFloat => Create<Half>(width, height, samples),
			16 when isSigned => Create<short>(width, height, samples),
			16 => Create<ushort>(width, height, samples),
			32 when isFloat => Create<float>(width, height, samples),
			32 when isSigned => Create<int>(width, height, samples),
			32 => Create<uint>(width, height, samples),
			64 when isFloat => Create<double>(width, height, samples),
			64 when isSigned => Create<long>(width, height, samples),
			64 => Create<ulong>(width, height, samples),
			_ => throw new NotSupportedException(),
		};

	static IImageBuffer Create<T>(Point<int> size, int samples) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => Create<T>(size.X, size.Y, samples);

	static IImageBuffer Create<T>(int width, int height, int samples) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> =>
		samples switch {
			1 => new ImageBuffer<ColorR<T>, T>(width, height),
			2 => new ImageBuffer<ColorRG<T>, T>(width, height),
			3 => new ImageBuffer<ColorRGB<T>, T>(width, height),
			4 => new ImageBuffer<ColorRGBA<T>, T>(width, height),
			_ => throw new NotSupportedException(),
		};
}
