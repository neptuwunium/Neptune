// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Triton.IO;
using Triton.Pixel;
using Triton.Pixel.Formats;

namespace Triton;

public record struct Point(int X, int Y);

public record struct Rect(Point TopLeft, Point WidthHeight);

public sealed class ImageBuffer<TColor, T>(IMemoryOwner<byte> buffer, int width, int height, bool overrideIsSigned = false) : IImageBuffer
	where TColor : unmanaged, IColor<TColor, T>, IColor<T>, IColor
	where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public ImageBuffer(IMemoryOwner<byte> buffer, Point size, bool overrideIsSigned = false) : this(buffer, size.X, size.Y, overrideIsSigned) { }
	public ImageBuffer(int width, int height) : this(new SizedMemoryOwner<byte>(Unsafe.SizeOf<TColor>() * width * height), width, height) => Clear();
	public ImageBuffer(Point size) : this(new SizedMemoryOwner<byte>(Unsafe.SizeOf<TColor>() * size.X * size.Y), size.X, size.Y) => Clear();

	public IMemoryOwner<TColor> ColorData { get; } = new TypedMemory<TColor>(buffer, 0);
	public IMemoryOwner<T> ValueData { get; } = new TypedMemory<T>(buffer, 0);

	public IMemoryOwner<byte> Data { get; } = buffer;
	public int Width { get; } = width;
	public int Height { get; } = height;
	public int Stride { get; } = Unsafe.SizeOf<TColor>();
	public int Components { get; } = Unsafe.SizeOf<TColor>() / Unsafe.SizeOf<T>();
	public bool IsHDR { get; } = typeof(T) == typeof(float) || typeof(T) == typeof(Half);
	public bool IsSigned { get; } = overrideIsSigned || typeof(T) == typeof(sbyte) || typeof(T) == typeof(short) || typeof(T) == typeof(int);
	public int BitDepth { get; } = Unsafe.SizeOf<T>() << 3;

	public IImageBuffer Cast<TNewColor, TNew>()
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor<TNew>, IColor
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
		Components switch {
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
	public IImageBuffer CreateSubImage(Point size) => new ImageBuffer<TColor, T>(size.X, size.Y);

	public void PremulitplyAlpha() {
		if (TColor.HasAlphaChannel) {
			foreach (ref var pixel in ColorData.Memory.Span) {
				PixelOperations<TColor, T>.PremultiplyPixel(ref pixel);
			}
		}
	}

	public void UnmultiplyAlpha() {
		if (TColor.HasAlphaChannel) {
			foreach (ref var pixel in ColorData.Memory.Span) {
				PixelOperations<TColor, T>.UnmultiplyPixel(ref pixel);
			}
		}
	}

	IColor IImageBuffer.Sample(float x, float y, SamplingOperation operation) => Sample(x, y, operation);

	public void Draw(IColor pixel, int x, int y, PixelOperation operation = PixelOperation.Copy) => Draw(pixel, new Point(x, y), operation);

	public void Draw(IColor pixel, Point target, PixelOperation operation = PixelOperation.Copy) {
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

	public void Draw(IImageBuffer image, int x, int y, PixelOperation operation = PixelOperation.Copy) => Draw(image, new Point(x, y), new Rect(default, new Point(image.Width, image.Height)), operation);
	public void Draw(IImageBuffer image, Point target, PixelOperation operation = PixelOperation.Copy) => Draw(image, target, new Rect(default, new Point(image.Width, image.Height)), operation);

	public void Draw(IImageBuffer image, Point target, Rect crop, PixelOperation operation = PixelOperation.Copy) {
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
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor<TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> {
		var newPixel = color.Convert<TNewColor, TNew, TColor, T>();
		foreach (ref var pixel in ColorData.Memory.Span) {
			PixelOperations<TColor, T>.BlendPixel(operation, newPixel, ref pixel);
		}
	}

	public void Clear<TNewColor, TNew>(TNewColor color, int x, int y, PixelOperation operation = PixelOperation.Copy)
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor<TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> => Clear<TNewColor, TNew>(color, new Rect(new Point(x, y), new Point(Width, Height)), operation);

	public void Clear<TNewColor, TNew>(TNewColor color, Point target, PixelOperation operation = PixelOperation.Copy)
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor<TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> => Clear<TNewColor, TNew>(color, new Rect(target, new Point(Width, Height)), operation);

	public void Clear<TNewColor, TNew>(TNewColor color, Rect target, PixelOperation operation = PixelOperation.Copy)
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor<TNew>, IColor
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
}
