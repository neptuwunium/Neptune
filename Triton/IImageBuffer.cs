// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Numerics;
using Triton.Pixel;
using Triton.Pixel.Formats;

namespace Triton;

public interface IImageBuffer : IDisposable {
	public IMemoryOwner<byte> Data { get; }
	public int Width { get; }
	public int Height { get; }
	public int Stride { get; }
	public int Components { get; }
	public bool IsHDR { get; }
	public bool IsSigned { get; }
	public int BitDepth { get; }

	public IImageBuffer Cast<TNewColor, TNew>()
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor<TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew>;

	public IImageBuffer Cast<TNew>()
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew>;

	public IImageBuffer CreateSubImage(int width, int height);

	public IImageBuffer CreateSubImage(Point size);

	public void Draw(IImageBuffer image, int x, int y, ImageDrawOperation op = ImageDrawOperation.Copy);
	public void Draw(IImageBuffer image, Point target, ImageDrawOperation op = ImageDrawOperation.Copy);
	public void Draw(IImageBuffer image, Point target, Rect crop, ImageDrawOperation op = ImageDrawOperation.Copy);

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
			_ => throw new NotSupportedException()
		};

	static IImageBuffer Create<T>(Point size, int samples) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => Create<T>(size.X, size.Y, samples);

	static IImageBuffer Create<T>(int width, int height, int samples) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> =>
		samples switch {
			1 => new ImageBuffer<ColorR<T>, T>(width, height),
			2 => new ImageBuffer<ColorRG<T>, T>(width, height),
			3 => new ImageBuffer<ColorRGB<T>, T>(width, height),
			4 => new ImageBuffer<ColorRGBA<T>, T>(width, height),
			_ => throw new NotSupportedException(),
		};
}
