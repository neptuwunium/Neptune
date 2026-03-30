// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using Pluto.Maths;

namespace Triton;

public sealed class IBLImage : IDisposable {
	// DXGI Order.
	private static readonly Point<int>[] CrossToTile = [
		new(2, 1), // X+
		new(0, 1), // X-
		new(1, 0), // Y+
		new(1, 2), // Y-
		new(1, 1), // Z+
		new(3, 1), // Z-
	];

	public IBLImage(ImageCollection frames, CubemapOrder? order = null, bool leaveOpen = false) {
		if (frames.Count != 6) {
			throw new InvalidOperationException("Provided Image Collection must be at least 6 frames");
		}

		Frames = frames;
		Order = order ?? CubemapOrder.DXGIOrder;
		Size = frames[0].Width;
		LeaveOpen = leaveOpen;
		if (!frames.All(x => x.Width == Size && x.Height == Size)) {
			throw new InvalidOperationException("All frames must be of equal size");
		}
	}

	public ImageCollection Frames { get; }
	public int Size { get; }
	public CubemapOrder Order { get; }
	public bool LeaveOpen { get; }

	public void Dispose() {
		if (LeaveOpen) {
			return;
		}

		Frames.Dispose();
	}

	public static IBLImage FromCross(IImageBuffer image, CubemapOrder? order) {
		var size = image.Width / 4;
		if (size != image.Height / 3) {
			throw new InvalidOperationException("Image must be 4:3");
		}

		return FromCrop(image, size, order, CrossToTile);
	}

	public static IBLImage FromCrop(IImageBuffer image, int size, CubemapOrder? order, params Point<int>[] crops) {
		if (crops.Length != 6) {
			throw new InvalidOperationException("Must have six crop factors");
		}

		var collection = new ImageCollection(6);

		var orderSelector = order ?? CubemapOrder.DXGIOrder;
		var rect = new Point<int>(size, size);
		for (var i = 0; i < 6; ++i) {
			var (tileX, tileY) = crops[orderSelector[i]];
			var frame = image.CreateSubImage(rect);
			frame.Draw(image, new Point<int>(), new Rect<int>(new Point<int>(tileX * size, tileY * size), rect));
			collection.Add(frame);
		}

		return new IBLImage(collection, CubemapOrder.DXGIOrder);
	}

	public IImageBuffer ToCross() {
		var crossImage = Frames[0].CreateSubImage(Size * 4, Size * 3);
		crossImage.Draw(Frames[Order.PositiveY], Size, 0); // Y+
		crossImage.Draw(Frames[Order.NegativeX], 0, Size); // -X
		crossImage.Draw(Frames[Order.PositiveZ], Size, Size); // +Z
		crossImage.Draw(Frames[Order.PositiveX], Size * 2, Size); // +X
		crossImage.Draw(Frames[Order.NegativeZ], Size * 3, Size); // -Z
		crossImage.Draw(Frames[Order.NegativeY], Size, Size * 2); //-Y
		return crossImage;
	}

	// from https://github.com/syoyo/tinyexr/blob/release/examples/cube2longlat/cube2longlat.cc
	private static int LocatePointOnCuboid(Vector3 coordinate, out Vector2 uv) {
		var absX = Math.Abs(coordinate.X);
		var absY = Math.Abs(coordinate.Y);
		var absZ = Math.Abs(coordinate.Z);

		var isXPositive = coordinate.X > 0.0f;
		var isYPositive = coordinate.Y > 0.0f;
		var isZPositive = coordinate.Z > 0.0f;

		var maxAxis = 0f;
		var uc = 0f;
		var vc = 0f;
		var index = 0;

		switch (isXPositive) {
			case true when absX >= absY && absX >= absZ:
				maxAxis = absX;
				uc = -coordinate.Z;
				vc = coordinate.Y;
				index = 0;
				break;
			case false when absX >= absY && absX >= absZ:
				maxAxis = absX;
				uc = coordinate.Z;
				vc = coordinate.Y;
				index = 1;
				break;
		}

		switch (isYPositive) {
			case true when absY >= absX && absY >= absZ:
				maxAxis = absY;
				uc = coordinate.X;
				vc = -coordinate.Z;
				index = 2;
				break;
			case false when absY >= absX && absY >= absZ:
				maxAxis = absY;
				uc = coordinate.X;
				vc = coordinate.Z;
				index = 3;
				break;
		}

		switch (isZPositive) {
			case true when absZ >= absX && absZ >= absY:
				maxAxis = absZ;
				uc = coordinate.X;
				vc = coordinate.Y;
				index = 4;
				break;
			case false when absZ >= absX && absZ >= absY:
				maxAxis = absZ;
				uc = -coordinate.X;
				vc = coordinate.Y;
				index = 5;
				break;
		}

		uv = new Vector2(0.5f * (uc / maxAxis + 1.0f), 0.5f * (vc / maxAxis + 1.0f));
		return index;
	}

	public IImageBuffer ToEquirectangular(SamplingOperation samplingOperation = SamplingOperation.Bilinear, double phiOffset = 0d) {
		var image = Frames[0].CreateSubImage(Size * 4, Size * 2); // 6, 3? what is the appropriate scale here?
		var w = (double) image.Width;
		var h = (double) image.Height;
		var phiOffsetRad = phiOffset * Math.PI / 180d;

		for (var y = 0; y < image.Height; ++y) {
			var theta = (y + 0.5d) / h * Math.PI;
			for (var x = 0; x < image.Width; ++x) {
				var phi = (x + 0.5d) / w * 2.0d * Math.PI + phiOffsetRad;
				var coordinate = new Vector3((float) (Math.Sin(theta) * Math.Cos(phi)), (float) Math.Cos(theta), (float) (-Math.Sin(theta) * Math.Sin(phi)));
				var face = LocatePointOnCuboid(coordinate, out var uv);
				var frame = Frames[Order[face]];
				image.Draw(frame.Sample(uv.X, uv.Y, samplingOperation), x, y);
			}
		}

		return image;
	}

	public IImageBuffer Convert(CubemapStyle style) =>
		style switch {
			CubemapStyle.Cross => ToCross(),
			CubemapStyle.Equirectangular => ToEquirectangular(),
			_ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
		};
}
