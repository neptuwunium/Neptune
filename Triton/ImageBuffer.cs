// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using System.Runtime.CompilerServices;
using Pluto.IO.Binary;
using Pluto.Maths;
using Triton.Pixel;
using Triton.Pixel.Formats;

namespace Triton;

public sealed class ImageBuffer<TColor, T> : IImageBuffer
	where TColor : unmanaged, IColor<TColor, T>, IColor
	where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public ImageBuffer(IRentedArray<byte> buffer, Point<int> size, bool overrideIsSigned = false) : this(buffer, size.X, size.Y, overrideIsSigned) { }
	public ImageBuffer(int width, int height) : this(new RentedArray<byte>(Unsafe.SizeOf<TColor>() * width * height), width, height) => Clear();
	public ImageBuffer(Point<int> size) : this(new RentedArray<byte>(Unsafe.SizeOf<TColor>() * size.X * size.Y), size.X, size.Y) => Clear();
	public ImageBuffer() : this(RentedArray<byte>.Empty, 0, 0) { }

	public ImageBuffer(IRentedArray<byte> buffer, int width, int height, bool? overrideIsSigned = null) {
		ColorData = new UnownedCovariantArray<TColor>(buffer);
		ValueData = new UnownedCovariantArray<T>(buffer);
		Data = buffer;
		Width = width;
		Height = height;
		Size = new Point<int>(width, height);
		ColorId = IColor<TColor, T>.ColorId;
		Compression = ImageCompression.Linear;
		if (overrideIsSigned.HasValue) {
			ColorId = ColorId with { IsSigned = overrideIsSigned.Value };
		}
	}

	public IRentedArray<TColor> ColorData { get; }
	public IRentedArray<T> ValueData { get; }

	public ref TColor this[int key] => ref ColorData.Span[key];
	public ref TColor this[int x, int y] => ref ColorData.Span[y * Width + x];

	public IRentedArray<byte> Data { get; }
	public int Width { get; }
	public int Height { get; }
	public Point<int> Size { get; }
	public int Stride { get; } = Unsafe.SizeOf<TColor>();
	public ColorId ColorId { get; }
	public ImageCompression Compression { get; }

	public ImageBuffer<TNewColor, TNew> Cast<TNewColor, TNew>()
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> {
		if (typeof(TNewColor) == typeof(TColor) && typeof(TNew) == typeof(T)) {
			return (ImageBuffer<TNewColor, TNew>) (object) this;
		}

		var buffer = new ImageBuffer<TNewColor, TNew>(Width, Height);
		RgbConverter.Convert<TColor, T, TNewColor, TNew>(ColorData.Span, buffer.ColorData.Span);
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

	IImageBuffer IImageBuffer.CreateSubImage(int width, int height) => new ImageBuffer<TColor, T>(width, height);
	IImageBuffer IImageBuffer.CreateSubImage(Point<int> size) => new ImageBuffer<TColor, T>(size.X, size.Y);

	public void PremultiplyAlpha() {
		if (!TColor.HasAlphaChannel) {
			return;
		}

		foreach (ref var pixel in ColorData.Span) {
			PixelOperations<TColor, T>.PremultiplyPixel(ref pixel);
		}
	}

	public void StraightAlpha() {
		if (!TColor.HasAlphaChannel) {
			return;
		}

		foreach (ref var pixel in ColorData.Span) {
			PixelOperations<TColor, T>.UnmultiplyPixel(ref pixel);
		}
	}

	IColor IImageBuffer.Sample(float x, float y, SamplingOperation operation, SamplingWrap wrap) => Sample(x, y, operation, wrap);
	IColor IImageBuffer.Sample(Point<float> target, SamplingOperation operation, SamplingWrap wrap) => Sample(target, operation, wrap);

	IImageBuffer IImageBuffer.Resize(int x, int y, SamplingOperation operation) => Resize(x, y, operation);
	IImageBuffer IImageBuffer.Resize(Point<int> target, SamplingOperation operation) => Resize(target, operation);

	IImageBuffer IImageBuffer.Rotate(float degrees, float? x, float? y, SamplingOperation operation) => Rotate(degrees, x, y, operation);
	IImageBuffer IImageBuffer.Rotate(float degrees, Point<float>? target, SamplingOperation operation) => Rotate(degrees, target, operation);

	public void Alpha(float alpha, int x, int y, PixelOperation operation = PixelOperation.Copy) => Alpha(alpha, new Point<int>(x, y), operation);

	public void Alpha(float alpha, Point<int> target, PixelOperation operation = PixelOperation.Copy) {
		if (target.X > Width || target.X < 0) {
			return;
		}

		if (target.Y > Height || target.Y < 0) {
			return;
		}

		var dstImagePixels = ColorData.Span;
		ref var dstPixel = ref dstImagePixels[target.Y * Width + target.X];
		var copy = dstPixel;
		var tF = alpha * float.CreateSaturating(TColor.White.A);
		PixelOperations<TColor, T>.BlendPixel(operation, TColor.Transparent with { A = T.CreateSaturating(tF) }, ref copy);
		dstPixel.A = copy.A;
	}

	public void Clear(int x, int y, PixelOperation operation = PixelOperation.Copy) => Clear(new Point<int>(x, y), operation);

	public void Clear(Point<int> target, PixelOperation operation = PixelOperation.Copy) {
		if (target.X > Width || target.X < 0) {
			return;
		}

		if (target.Y > Height || target.Y < 0) {
			return;
		}

		var dstImagePixels = ColorData.Span;
		if (TColor.HasAlphaChannel) {
			ref var dstPixel = ref dstImagePixels[target.Y * Width + target.X];
			dstPixel.A = TColor.Transparent.A;
		} else {
			dstImagePixels[target.Y * Width + target.X] = TColor.Transparent;
		}
	}

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

		var dstImagePixels = ColorData.Span;
		ref var dstPixel = ref dstImagePixels[target.Y * Width + target.X];
		PixelOperations<TColor, T>.BlendPixel(operation, colorPixel, ref dstPixel);
	}

	public void Draw(IImageBuffer image, int x, int y, PixelOperation operation = PixelOperation.Copy) => Draw(image, new Point<int>(x, y), new Rect<int>(0, new Point<int>(image.Width, image.Height)), operation);
	public void Draw(IImageBuffer image, Point<int> target, PixelOperation operation = PixelOperation.Copy) => Draw(image, target, new Rect<int>(0, new Point<int>(image.Width, image.Height)), operation);

	public void Draw(IImageBuffer image, Point<int> target, Rect<int> crop, PixelOperation operation = PixelOperation.Copy) {
		ImageBuffer<TColor, T>? convertedImage = null;
		if (image is not ImageBuffer<TColor, T> imageBuffer) {
			imageBuffer = image.Cast<TColor, T>();
			convertedImage = imageBuffer;
		}

		var (x, y) = target;
		var ((cropX, cropY), (cropW, cropH)) = crop;

		try {
			var dstImagePixels = ColorData.Span;
			var srcImagePixels = imageBuffer.ColorData.Span;

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

	IImageBuffer IImageBuffer.Clone() => Clone();

	public void Clear() => Clear<TColor, T>(TColor.Transparent);
	public void Clear(Rect<int> area) => Clear<TColor, T>(TColor.Transparent, area);

	public void Clear<TNewColor, TNew>(TNewColor color, PixelOperation operation = PixelOperation.Copy)
		where TNewColor : unmanaged, IColor<TNewColor, TNew>, IColor
		where TNew : unmanaged, INumberBase<TNew>, IMinMaxValue<TNew> {
		var newPixel = color.Convert<TNewColor, TNew, TColor, T>();
		foreach (ref var pixel in ColorData.Span) {
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
		var dstImagePixels = ColorData.Span;

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

	public void Flip() {
		var sp = ColorData.Span;
		for (var y = 0; y < Height; y++) {
			for (var x = 0; x < Width / 2; x++) {
				var left = y * Width + x;
				var right = y * Width + (Width - 1 - x);
				(sp[left], sp[right]) = (sp[right], sp[left]);
			}
		}
	}

	public void Flop() {
		var sp = ColorData.Span;
		for (var y = 0; y < Height / 2; y++) {
			for (var x = 0; x < Width; x++) {
				var left = y * Width + x;
				var right = (Height - 1 - y) * Width + x;
				(sp[left], sp[right]) = (sp[right], sp[left]);
			}
		}
	}

	public ImageBuffer<TColor, T> CreateSubImage(int width, int height) => new(width, height);
	public ImageBuffer<TColor, T> CreateSubImage(Point<int> size) => new(size.X, size.Y);

	public ImageBuffer<TColor, T> Clone() {
		var image = new ImageBuffer<TColor, T>(Size);
		Data.Memory.CopyTo(image.Data.Memory);
		return image;
	}

	public TColor Sample(float x, float y, SamplingOperation operation = SamplingOperation.Bilinear, SamplingWrap wrap = SamplingWrap.Repeat) => Sample(new Point<float>(x, y), operation, wrap);

	public TColor Sample(Point<float> target, SamplingOperation operation = SamplingOperation.Bilinear, SamplingWrap wrap = SamplingWrap.Repeat) {
		var (x, y) = target;

		if (x > Width || x < 0) {
			return TColor.Transparent;
		}

		if (y > Height || y < 0) {
			return TColor.Transparent;
		}

		return SamplingOperations<TColor, T>.SamplePixel(operation, wrap, x, y, this);
	}

	public ImageBuffer<TColor, T> Resize(Point<int> target, SamplingOperation operation = SamplingOperation.Bilinear) => Resize(target.X, target.Y, operation);

	public ImageBuffer<TColor, T> Resize(int width, int height, SamplingOperation operation = SamplingOperation.Bilinear) {
		if (width == Width && height == Height) {
			return Clone();
		}

		var newImage = new ImageBuffer<TColor, T>(width, height);

		var s = new Point<float>(Width, Height) / width;
		var spt = newImage.ColorData.Span;

		for (var y = 0; y < height; y++) {
			for (var x = 0; x < width; x++) {
				var p = (new Point<float>(x, y) + 0.5f) * s - 0.5f;
				spt[y * Width + x] = Sample(p, operation, SamplingWrap.Extend);
			}
		}

		return newImage;
	}

	public ImageBuffer<TColor, T> Rotate(float degrees, float? x = null, float? y = null, SamplingOperation operation = SamplingOperation.Bilinear, SamplingWrap wrap = SamplingWrap.Extend) => Rotate(degrees, new Point<float>(x ?? Width / 2f, y ?? Height / 2f), operation, wrap);

	public ImageBuffer<TColor, T> Rotate(float degrees, Point<float>? pivot = null, SamplingOperation operation = SamplingOperation.Bilinear, SamplingWrap wrap = SamplingWrap.Extend) {
		var radians = degrees * (MathF.PI / 180f);

		var newImage = new ImageBuffer<TColor, T>(Size);

		var p = pivot ?? new Point<float>(Width, Height) / 2f;
		var cos = MathF.Cos(-radians);
		var sin = MathF.Sin(-radians);

		for (var y = 0; y < Height; y++) {
			for (var x = 0; x < Width; x++) {
				var d = new Point<float>(x, y) - p;
				var sx = d.X * cos - d.Y * sin + p.X;
				var sy = d.X * sin + d.Y * cos + p.Y;
				newImage[x, y] = Sample(sx, sy, operation);
			}
		}

		return newImage;
	}
}
