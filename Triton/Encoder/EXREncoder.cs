// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton.Encoder;

public class EXREncoder : IEncoder {
	public void Write(Stream stream, EncoderWriteOptions options, ImageCollection frames) => throw new NotImplementedException();
	public ImageCollection Read(Stream stream) => throw new NotImplementedException();
}
