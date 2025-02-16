// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Channels;

public readonly record struct G : IChannel {
	static bool IChannel.IsGreen => true;
}
