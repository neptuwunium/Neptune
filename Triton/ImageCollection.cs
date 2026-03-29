// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton;

public sealed class ImageCollection : List<IImageBuffer>, IDisposable {
	public ImageCollection(int capacity) : base(capacity) { }
	public ImageCollection() { }

	public void Dispose() {
		foreach (var frame in this) {
			frame.Dispose();
		}
	}
}
