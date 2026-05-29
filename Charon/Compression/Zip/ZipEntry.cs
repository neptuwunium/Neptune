// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Collections;
using Pluto.IO.Binary;

namespace Charon.Compression.Zip;

public record ZipEntry : IDisposable {
	public required string Path { get; set; }
	public required long Length { get; set; }
	public required string Comment { get; set; }
	public required ZipCentralDirectoryHeader Header { get; set; }
	public ZipEntryExtra? Extra { get; set; }

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			Extra?.Dispose();
		}
	}

	~ZipEntry() => Dispose(false);
}

public record ZipEntryExtra : IDisposable, IEnumerable<(ushort Id, UnownedRentedArray<byte> Buffer)> {
	public IRentedArray<byte>? Data { get; set; }

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public IEnumerator<(ushort Id, UnownedRentedArray<byte> Buffer)> GetEnumerator() {
		if (Data == null || Data.Length == 0) {
			yield break;
		}

		using var reader = new ArrayPoolBinaryReader(Data, true);
		while (reader.Unconsumed > 4) {
			var header = reader.Read<ZipExtraHeader>();
			var buffer = (UnownedRentedArray<byte>) reader.ReadSharedBytes(header.Length);
			yield return (header.Id, buffer);
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


	protected virtual void Dispose(bool disposing) {
		if (disposing) {
			Data?.Dispose();
		}
	}

	~ZipEntryExtra() => Dispose(false);
}
