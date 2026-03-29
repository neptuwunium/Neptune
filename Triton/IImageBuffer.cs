// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Numerics;
using Triton.Pixel;
using Triton.Pixel.Formats;

namespace Triton;

public interface IImageBuffer : IDisposable {
	IMemoryOwner<byte> Data { get; }
	int Width { get; }
	int Height { get; }
	Point<int> Size { get; }
	int Stride { get; }
	ColorId ColorId { get; }

	ImageBuffer<TNewColor, TNew> Cast<TNewColor, TNew>()
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew>;

	IImageBuffer Cast<TNew>()
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew>;

	IImageBuffer Cast(int components);

	IImageBuffer CreateSubImage(int width, int height);

	IImageBuffer CreateSubImage(Point<int> size);

	void PremultiplyAlpha();
	void StraightAlpha();

	IColor Sample(float x, float y, SamplingOperation operation = SamplingOperation.Bilinear, SamplingWrap wrap = SamplingWrap.Repeat);
	IColor Sample(Point<float> target, SamplingOperation operation = SamplingOperation.Bilinear, SamplingWrap wrap = SamplingWrap.Repeat);

	IImageBuffer Resize(int x, int y, SamplingOperation operation = SamplingOperation.Bilinear);
	IImageBuffer Resize(Point<int> target, SamplingOperation operation = SamplingOperation.Bilinear);

	IImageBuffer Rotate(float degrees, float? x = null, float? y = null, SamplingOperation operation = SamplingOperation.Bilinear);
	IImageBuffer Rotate(float degrees, Point<float>? pivot = null, SamplingOperation operation = SamplingOperation.Bilinear);

	void Alpha(float alpha, int x, int y, PixelOperation operation = PixelOperation.Copy);
	void Alpha(float alpha, Point<int> target, PixelOperation operation = PixelOperation.Copy);
	void Clear(int x, int y, PixelOperation operation = PixelOperation.Copy);
	void Clear(Point<int> target, PixelOperation operation = PixelOperation.Copy);
	void Draw(IColor pixel, int x, int y, PixelOperation operation = PixelOperation.Copy);
	void Draw(IColor pixel, Point<int> target, PixelOperation operation = PixelOperation.Copy);
	void Draw(IImageBuffer image, int x, int y, PixelOperation operation = PixelOperation.Copy);
	void Draw(IImageBuffer image, Point<int> target, PixelOperation operation = PixelOperation.Copy);
	void Draw(IImageBuffer image, Point<int> target, Rect<int> crop, PixelOperation operation = PixelOperation.Copy);

	void Flip();
	void Flop();

	void Clear();
	void Clear(Rect<int> area);
	IImageBuffer Clone();

	void Clear<TColor, T>(TColor color, PixelOperation operation = PixelOperation.Copy)
		where TColor : unmanaged, IColor<TColor, T>, IColor
		where T : unmanaged, INumberBase<T>, IMinMaxValue<T>;

	void Clear<TColor, T>(TColor color, int x, int y, PixelOperation operation = PixelOperation.Copy)
		where TColor : unmanaged, IColor<TColor, T>, IColor
		where T : unmanaged, INumberBase<T>, IMinMaxValue<T>;

	void Clear<TColor, T>(TColor color, Point<int> target, PixelOperation operation = PixelOperation.Copy)
		where TColor : unmanaged, IColor<TColor, T>, IColor
		where T : unmanaged, INumberBase<T>, IMinMaxValue<T>;

	void Clear<TColor, T>(TColor color, Rect<int> target, PixelOperation operation = PixelOperation.Copy)
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
