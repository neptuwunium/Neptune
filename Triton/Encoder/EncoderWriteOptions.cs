// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton.Encoder;

public record EncoderWriteOptions {
	public static EncoderWriteOptions Default { get; } = new();

	public bool Compress { get; init; } = true;
	public bool AssociateAlpha { get; init; }
}
