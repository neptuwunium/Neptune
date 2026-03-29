// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Triton;

[Flags]
public enum ChannelLayout : byte {
	RedFirst = 0,
	GreenFirst = 1,
	BlueFirst = 2,
	AlphaRedFirst = 3,
	AlphaGreenFirst = 4,
	AlphaBlueFirst = 4,
}
