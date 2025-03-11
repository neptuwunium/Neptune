// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton.Encoder;

public interface IEncoder {
	public static abstract bool IsAvailable { get; }
	public void Write(Stream stream, EncoderWriteOptions options, ImageCollection frames);
	public ImageCollection Read(Stream stream);
}
