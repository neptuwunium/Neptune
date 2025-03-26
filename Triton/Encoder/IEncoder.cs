// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2 OR LGPL-3.0-or-later
// You may choose either license when using or modifying this code.

namespace Triton.Encoder;

public interface IEncoder {
	public static abstract bool IsAvailable { get; }
	public void Write(Stream stream, EncoderWriteOptions options, ImageCollection frames);
	public ImageCollection Read(Stream stream);
}
