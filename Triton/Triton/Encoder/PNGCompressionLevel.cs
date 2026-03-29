// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2 OR LGPL-3.0-or-later
// You may choose either license when using or modifying this code.

namespace Triton.Encoder;

public enum PNGCompressionLevel {
	None = 0,
	Level1 = 1,
	Level2 = 2,
	Level3 = 3,
	Level4 = 4,
	Level5 = 5,
	Level6 = 6,
	Level7 = 7,
	Level8 = 8,
	Level9 = 9,

	SuperFast = Level1,
	Fast = Level3,
	SuperSmall = Level9,
	Small = Level6,
}
