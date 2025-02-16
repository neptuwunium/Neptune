// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton.Encoder;

public interface IEncoder {
	public void Write(Stream stream, ImageCollection frames);
	public ImageCollection Read(Stream stream);
}
