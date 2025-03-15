// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using Triton.Pixel;
using Triton.Pixel.Formats;

namespace Triton;

public static class SamplingOperations<TColor, T>
	where TColor : unmanaged, IColor<TColor, T>, IColor
	where T : unmanaged, INumberBase<T>, IMinMaxValue<T> {
	public static TColor SamplePixel(SamplingOperation operation, float x, float y, ImageBuffer<TColor, T> image) {
		x *= image.Width;
		y *= image.Height;
		y = image.Height - y;

		if (Handle1D(ref x, ref y, image, out var samplePixel)) {
			return samplePixel;
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

	public static bool Handle1D(ref float x, ref float y, ImageBuffer<TColor, T> image, out TColor samplePixel) {
		if (image.Width == 0 || image.Height == 0) {
			samplePixel = TColor.Transparent;
			return true;
		}

		// todo: wrap enumeration? atm it "repeats".
		x %= image.Width;
		y %= image.Height;

		var sx = (int) Math.Round(x);
		var sy = (int) Math.Round(y);

		if (image.Width == 1 || image.Height == 1) {
			samplePixel = image.ColorData.Memory.Span[sy * image.Width + sx];
			return true;
		}

		samplePixel = default;
		return false;
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
