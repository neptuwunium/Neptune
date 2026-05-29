// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Compression.Zip;

public enum ZipCompression : ushort {
	Store = 0,
	Deflate = 8,
	Deflate64 = 9,
}
