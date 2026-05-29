// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Charon.Compression.Zip;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct ZipExtraHeader {
	public const ushort ZIP64_EXTRA_HEADER_ID = 0x0001;

	public ushort Id { get; init; }
	public ushort Length { get; init; }
}
