// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2 OR LGPL-3.0-or-later
// You may choose either license when using or modifying this code.

namespace Triton.Encoder;

public class JPEGEncoder : IEncoder {
	public static bool IsAvailable { get; } = false;
	public void Write(Stream stream, EncoderWriteOptions options, ImageCollection frames) => throw new NotImplementedException();
	public ImageCollection Read(Stream stream) => throw new NotImplementedException();
}
