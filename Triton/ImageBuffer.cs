// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Triton.IO;
using Triton.Pixel;
using Triton.Pixel.Channels;
using Triton.Pixel.Formats;

namespace Triton;

public sealed class ImageBuffer<TColor, T> : IImageBuffer
	where TColor : unmanaged, IColor<TColor, T>, IColor
	where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public ImageBuffer(IMemoryOwner<byte> buffer, Point<int> size, bool overrideIsSigned = false) : this(buffer, size.X, size.Y, overrideIsSigned) { }
	public ImageBuffer(int width, int height) : this(new SizedMemoryOwner<byte>(Unsafe.SizeOf<TColor>() * width * height), width, height) => Clear();
	public ImageBuffer(Point<int> size) : this(new SizedMemoryOwner<byte>(Unsafe.SizeOf<TColor>() * size.X * size.Y), size.X, size.Y) => Clear();

	public ImageBuffer(IMemoryOwner<byte> buffer, int width, int height, bool? overrideIsSigned = null) {
		ColorData = new TypedMemory<TColor>(buffer, 0);
		ValueData = new TypedMemory<T>(buffer, 0);
		Data = buffer;
		Width = width;
		Height = height;
		ColorId = ColorId.FromPixel<TColor, T>(overrideIsSigned);

		var type = typeof(TColor);

		IsCanonized = type == typeof(Color<T, R>) ||
			type == typeof(Color<T, G>) ||
			type == typeof(Color<T, B>) ||
			type == typeof(Color<T, A>) ||
			type == typeof(Color<T, R, G>) ||
			type == typeof(Color<T, R, G, B>) ||
			type == typeof(Color<T, R, G, B, A>) ||
			type == typeof(ColorA<T>) ||
			type == typeof(ColorR<T>) ||
			type == typeof(ColorRG<T>) ||
			type == typeof(ColorRGB<T>) ||
			type == typeof(ColorRGBA<T>);
	}

	public IMemoryOwner<TColor> ColorData { get; }
	public IMemoryOwner<T> ValueData { get; }

	public IMemoryOwner<byte> Data { get; }
	public int Width { get; }
	public int Height { get; }
	public int Stride { get; } = Unsafe.SizeOf<TColor>();
	public bool IsCanonized { get; }
	public ColorId ColorId { get; }

	public IImageBuffer Cast<TNewColor, TNew>()
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> {
		if (typeof(TNewColor) == typeof(TColor) && typeof(TNew) == typeof(T)) {
			return this;
		}

		var buffer = new ImageBuffer<TNewColor, TNew>(Width, Height);
		RgbConverter.Convert<TColor, T, TNewColor, TNew>(ColorData.Memory.Span, buffer.ColorData.Memory.Span);
		return buffer;
	}

	public IImageBuffer Cast<TNew>()
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> =>
		ColorId.Components switch {
			1 => Cast<ColorR<TNew>, TNew>(),
			2 => Cast<ColorRG<TNew>, TNew>(),
			3 => Cast<ColorRGB<TNew>, TNew>(),
			4 => Cast<ColorRGBA<TNew>, TNew>(),
			_ => throw new NotSupportedException(),
		};

	public IImageBuffer Cast(int components) =>
		components switch {
			1 => Cast<ColorR<T>, T>(),
			2 => Cast<ColorRG<T>, T>(),
			3 => Cast<ColorRGB<T>, T>(),
			4 => Cast<ColorRGBA<T>, T>(),
			_ => throw new NotSupportedException(),
		};

	public IImageBuffer CreateSubImage(int width, int height) => new ImageBuffer<TColor, T>(width, height);
	public IImageBuffer CreateSubImage(Point<int> size) => new ImageBuffer<TColor, T>(size.X, size.Y);

	public void PremultiplyAlpha() {
		if (!TColor.HasAlphaChannel) {
			return;
		}

		foreach (ref var pixel in ColorData.Memory.Span) {
			PixelOperations<TColor, T>.PremultiplyPixel(ref pixel);
		}
	}

	public void StraightAlpha() {
		if (!TColor.HasAlphaChannel) {
			return;
		}

		foreach (ref var pixel in ColorData.Memory.Span) {
			PixelOperations<TColor, T>.UnmultiplyPixel(ref pixel);
		}
	}

	IColor IImageBuffer.Sample(float x, float y, SamplingOperation operation) => Sample(x, y, operation);
	IColor IImageBuffer.Sample(Point<float> target, SamplingOperation operation) => Sample(target, operation);

	public void Draw(IColor pixel, int x, int y, PixelOperation operation = PixelOperation.Copy) => Draw(pixel, new Point<int>(x, y), operation);

	public void Draw(IColor pixel, Point<int> target, PixelOperation operation = PixelOperation.Copy) {
		if (pixel is not TColor colorPixel) {
			// todo: cast
			throw new InvalidOperationException("Invalid pixel format");
		}

		if (target.X > Width || target.X < 0) {
			return;
		}

		if (target.Y > Height || target.Y < 0) {
			return;
		}

		var dstImagePixels = ColorData.Memory.Span;
		ref var dstPixel = ref dstImagePixels[target.Y * Width + target.X];
		PixelOperations<TColor, T>.BlendPixel(operation, colorPixel, ref dstPixel);
	}

	public void Draw(IImageBuffer image, int x, int y, PixelOperation operation = PixelOperation.Copy) => Draw(image, new Point<int>(x, y), new Rect<int>(default, new Point<int>(image.Width, image.Height)), operation);
	public void Draw(IImageBuffer image, Point<int> target, PixelOperation operation = PixelOperation.Copy) => Draw(image, target, new Rect<int>(default, new Point<int>(image.Width, image.Height)), operation);

	public void Draw(IImageBuffer image, Point<int> target, Rect<int> crop, PixelOperation operation = PixelOperation.Copy) {
		ImageBuffer<TColor, T>? convertedImage = null;
		if (image is not ImageBuffer<TColor, T> imageBuffer) {
			imageBuffer = (ImageBuffer<TColor, T>) image.Cast<TColor, T>();
			convertedImage = imageBuffer;
		}

		var (x, y) = target;
		var ((cropX, cropY), (cropW, cropH)) = crop;

		try {
			var dstImagePixels = ColorData.Memory.Span;
			var srcImagePixels = imageBuffer.ColorData.Memory.Span;

			for (var sy = 0; sy < cropH; sy++) {
				var dy = y + sy;
				if (dy < 0 || dy >= Height || sy + cropY > imageBuffer.Height) {
					continue;
				}

				for (var sx = 0; sx < cropW; sx++) {
					var dx = x + sx;
					if (dx < 0 || dx >= Width || sx + cropX > imageBuffer.Width) {
						continue;
					}

					var srcIndex = (sy + cropY) * imageBuffer.Width + sx + cropX;
					var dstIndex = dy * Width + dx;

					ref var dstPixel = ref dstImagePixels[dstIndex];
					var srcPixel = srcImagePixels[srcIndex];
					PixelOperations<TColor, T>.BlendPixel(operation, srcPixel, ref dstPixel);
				}
			}
		} finally {
			convertedImage?.Dispose();
		}
	}

	public void Clear() => Clear<TColor, T>(TColor.Transparent);

	public void Clear<TNewColor, TNew>(TNewColor color, PixelOperation operation = PixelOperation.Copy)
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> {
		var newPixel = color.Convert<TNewColor, TNew, TColor, T>();
		foreach (ref var pixel in ColorData.Memory.Span) {
			PixelOperations<TColor, T>.BlendPixel(operation, newPixel, ref pixel);
		}
	}

	public void Clear<TNewColor, TNew>(TNewColor color, int x, int y, PixelOperation operation = PixelOperation.Copy)
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> => Clear<TNewColor, TNew>(color, new Rect<int>(new Point<int>(x, y), new Point<int>(Width, Height)), operation);

	public void Clear<TNewColor, TNew>(TNewColor color, Point<int> target, PixelOperation operation = PixelOperation.Copy)
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> => Clear<TNewColor, TNew>(color, new Rect<int>(target, new Point<int>(Width, Height)), operation);

	public void Clear<TNewColor, TNew>(TNewColor color, Rect<int> target, PixelOperation operation = PixelOperation.Copy)
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> {
		var pixel = color.Convert<TNewColor, TNew, TColor, T>();
		var ((x, y), (w, h)) = target;
		var dstImagePixels = ColorData.Memory.Span;

		for (var sy = 0; sy < h; sy++) {
			var dy = y + sy;
			if (dy < 0 || dy >= Height || sy + w > Height) {
				continue;
			}

			for (var sx = 0; sx < w; sx++) {
				var dx = x + sx;
				if (dx < 0 || dx >= Width || sx + x > Width) {
					continue;
				}

				var dstIndex = dy * Width + dx;
				ref var dstPixel = ref dstImagePixels[dstIndex];
				PixelOperations<TColor, T>.BlendPixel(operation, pixel, ref dstPixel);
			}
		}
	}

	public void Dispose() {
		Data.Dispose();
		ColorData.Dispose();
		ValueData.Dispose();
	}

	public TColor Sample(float x, float y, SamplingOperation operation = SamplingOperation.Bilinear) {
		if (x > Width || x < 0) {
			return TColor.Transparent;
		}

		if (y > Height || y < 0) {
			return TColor.Transparent;
		}

		return SamplingOperations<TColor, T>.SamplePixel(operation, x, y, this);
	}

	public TColor Sample(Point<float> target, SamplingOperation operation = SamplingOperation.Bilinear) => Sample(target.X, target.Y, operation);
}
