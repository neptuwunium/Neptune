// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using Pluto.IO.Binary;

namespace Triton.Surface;

public class KTX : IDisposable {
	// ReSharper disable 4 CanSimplifyStringEscapeSequence
	public static ReadOnlySpan<byte> Magic11 => "\xAB\x4B\x54\x58\x20\x31\x31\xBB\x0D\x0A\x1A\x0A\x01\x02\x03\x04"u8;
	public static ReadOnlySpan<byte> Magic20 => "\xAB\x4B\x54\x58\x20\x32\x30\xBB\x0D\x0A\x1A\x0A\x01\x02\x03\x04"u8;

	public KTX(IRentedArray<byte> buffer, bool leaveOpen = false) {
		Buffer = buffer;
		LeaveOpen = leaveOpen;
	}

	public bool LeaveOpen { get; }
	public IRentedArray<byte> Buffer { get; }

	~KTX() => Dispose(false);
	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			Buffer.Dispose();
		}
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public static bool IsKTX(ReadOnlySpan<byte> span) => span.Length >= 0x10 && (span[..16].SequenceEqual(Magic11) || span[..16].SequenceEqual(Magic20));
}
