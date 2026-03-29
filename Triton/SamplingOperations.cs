// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using Triton.Pixel;
using Triton.Pixel.Formats;

namespace Triton;

public static class SamplingOperations<TColor, T>
	where TColor : unmanaged, IColor<TColor, T>, IColor
	where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public static TColor SamplePixel(SamplingOperation operation, SamplingWrap wrap, float x, float y, ImageBuffer<TColor, T> image) {
		x *= image.Width;
		y *= image.Height;
		y = image.Height - y;
		x = Math.Clamp(x, 0, image.Width - 1);
		y = Math.Clamp(y, 0, image.Height - 1);

		switch (wrap) {
			case SamplingWrap.Repeat: {
				x = (x % image.Width + image.Width) % image.Width;
				y = (y % image.Height + image.Height) % image.Height;
				break;
			}
			case SamplingWrap.Clip: {
				if (x >= image.Width || y >= image.Height || x < 0 || y < 0) {
					return TColor.Transparent;
				}

				break;
			}
			case SamplingWrap.Extend: {
				x = Math.Clamp(x, 0, image.Width);
				y = Math.Clamp(y, 0, image.Height);
				break;
			}
			default: throw new ArgumentOutOfRangeException(nameof(wrap), wrap, null);
		}

		switch (operation) {
			case SamplingOperation.Nearest: {
				var nx = (int) Math.Min(Math.Round(x), image.Width - 1);
				var ny = (int) Math.Min(Math.Round(y), image.Height - 1);
				return image.ColorData.Memory.Span[ny * image.Width + nx];
			}
			case SamplingOperation.Bilinear: {
				return BilinearSample(x, y, image);
			}
			default: throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
		}
	}

	public static TColor BilinearSample(float x, float y, ImageBuffer<TColor, T> image) {
		var sx = (int) x;
		var sy = (int) y;

		var nx = Math.Min(sx + 1, image.Width - 1);
		var ny = Math.Min(sy + 1, image.Height - 1);
		var a = x - sx;
		var b = y - sy;

		var data = image.ColorData.Memory.Span;
		var xP = data[sy * image.Width + sx].Convert<TColor, T, ColorRGBA<float>, float>();
		var yP = data[sy * image.Width + nx].Convert<TColor, T, ColorRGBA<float>, float>();
		var zP = data[ny * image.Width + sx].Convert<TColor, T, ColorRGBA<float>, float>();
		var wP = data[ny * image.Width + nx].Convert<TColor, T, ColorRGBA<float>, float>();

		var pixel = new ColorRGBA<float> {
			R = BilinearSample(a, b, xP.R, yP.R, zP.R, wP.R),
			G = BilinearSample(a, b, xP.G, yP.G, zP.G, wP.G),
			B = BilinearSample(a, b, xP.B, yP.B, zP.B, wP.B),
			A = BilinearSample(a, b, xP.A, yP.A, zP.A, wP.A),
		};

		return pixel.Convert<ColorRGBA<float>, float, TColor, T>();
	}

	private static float BilinearSample(float a, float b, float x, float y, float z, float w) =>
		(1 - a) * (1 - b) * x
		+ a * (1 - b) * y
		+ (1 - a) * b * z
		+ a * b * w;
}
