// SPDX-FileCopyrightText: 2022 - 2026 ds5678
// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Channels;

public readonly record struct Gray : IChannel {
	static bool IChannel.IsRed => true;
	static bool IChannel.IsGreen => true;
	static bool IChannel.IsBlue => true;
	static bool IChannel.FullyUtilized => false;
}
