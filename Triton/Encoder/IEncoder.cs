// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton.Encoder;

public interface IEncoder {
	static abstract bool IsAvailable { get; }
	void Write(Stream stream, EncoderWriteOptions options, ImageCollection frames);
	ImageCollection Read(Stream stream);
}
