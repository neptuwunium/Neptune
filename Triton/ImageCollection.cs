// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2 OR LGPL-3.0-or-later
// You may choose either license when using or modifying this code.

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
