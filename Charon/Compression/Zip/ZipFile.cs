// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Text;
using Pluto;
using Pluto.IO.Binary;

namespace Charon.Compression.Zip;

public sealed class ZipFile : IDisposable {
	public ZipFile(string path) : this(MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read)) { }

	public ZipFile(MemoryMappedFile memoryMap) {
		MemoryMap = memoryMap;
		Entries = ObjectPool<List<ZipEntry>>.Rent();
		Entries.Clear();

		using var reader = new MemoryMapBinaryReader(memoryMap, leaveOpen: true);
		reader.Position = reader.Length - Unsafe.SizeOf<ZipEndOfCentralDirectory>();
		CentralDirectory = reader.Read<ZipEndOfCentralDirectory>();
		if (CentralDirectory.Magic != ZipEndOfCentralDirectory.MAGIC) {
			throw new InvalidDataException("Can't read end of central directory magic");
		}

		if (CentralDirectory.IsZip64) {
			reader.Position = reader.Length - Unsafe.SizeOf<ZipEndOfCentralDirectory>() - Unsafe.SizeOf<Zip64EndOfCentralDirectoryLocator>();
			CentralDirectoryLocator = reader.Read<Zip64EndOfCentralDirectoryLocator>();
			if (CentralDirectoryLocator.Magic != Zip64EndOfCentralDirectoryLocator.MAGIC) {
				throw new InvalidDataException("Can't read end of central directory locator magic");
			}

			reader.Position = CentralDirectoryLocator.Offset;
			CentralDirectory64 = reader.Read<Zip64EndOfCentralDirectory>();
			if (CentralDirectory64.Magic != Zip64EndOfCentralDirectory.MAGIC) {
				throw new InvalidDataException("Can't read end of central directory zip64 magic");
			}
		}

		var numberOfRecords = CentralDirectory.IsZip64 ? CentralDirectory64.NumberOfDirectoryRecords : CentralDirectory.NumberOfDirectoryRecords;
		reader.Position = CentralDirectory.IsZip64 ? CentralDirectory64.DirectoryOffset : CentralDirectory.DirectoryOffset;
		for (var i = 0L; i < numberOfRecords; ++i) {
			var record = reader.Read<ZipCentralDirectoryHeader>();
			if (record.Magic != ZipCentralDirectoryHeader.MAGIC) {
				throw new InvalidDataException("Can't read central directory header magic");
			}

			var name = reader.ReadCString<byte>(Encoding.UTF8, record.FileNameLength, true);
			var extraData = reader.ReadSharedBytes(record.ExtraFieldLength);
			var comment = reader.ReadCString<byte>(Encoding.UTF8, record.CommentLength, true);
			Entries.Add(new ZipEntry {
				Path = name,
				Length = record.MemorySize,
				Comment = comment,
				Header = record,
				Extra = new ZipEntryExtra {
					Data = extraData,
				},
			});
		}
	}

	public ZipEndOfCentralDirectory CentralDirectory { get; }
	public Zip64EndOfCentralDirectoryLocator CentralDirectoryLocator { get; }
	public Zip64EndOfCentralDirectory CentralDirectory64 { get; }
	public List<ZipEntry> Entries { get; private set; }
	public MemoryMappedFile MemoryMap { get; }

	public void Dispose() {
		MemoryMap.Dispose();

		foreach (var entry in Entries) {
			entry.Dispose();
		}

		Entries.Clear();
		ObjectPool<List<ZipEntry>>.Return(Entries);
		Entries = null!;
	}

	public IRentedArray<byte> Open(ZipEntry entry, bool tryDecompress = true) {
		var diskSize = (long) entry.Header.DiskSize;
		var memorySize = (long) entry.Header.MemorySize;
		var offset = (long) entry.Header.Offset;
		var zipInfoComposite = entry.Extra?.FirstOrDefault(x => x.Id == ZipExtraHeader.ZIP64_EXTRA_HEADER_ID).Buffer;
		if (zipInfoComposite != null) {
			var zipInfo = MemoryMarshal.Read<Zip64ExtendedInformation>(zipInfoComposite.Span);
			if (entry.Header.MemorySize == uint.MaxValue) {
				memorySize = zipInfo.MemorySize;
			}

			if (entry.Header.DiskSize == uint.MaxValue) {
				diskSize = zipInfo.DiskSize;
			}

			if (entry.Header.Offset == uint.MaxValue) {
				offset = zipInfo.Offset;
			}
		}

		using var reader = new MemoryMapBinaryReader(MemoryMap, offset, leaveOpen: true);
		var header = reader.Read<ZipFileHeader>();
		if (header.Magic != ZipFileHeader.MAGIC) {
			throw new InvalidDataException("Can't read file header magic");
		}

		reader.Position += header.ExtraFieldLength + header.FileNameLength;

		var diskBuffer = reader.ReadBytes(checked((int) diskSize));
		var compression = (ZipCompression) entry.Header.Compression;
		if (compression == ZipCompression.Store || !tryDecompress || !entry.Header.IsSupported) {
			return diskBuffer;
		}

		RentedArray<byte>? memoryBuffer = null;
		try {
			if ((entry.Header.Flags & 0b1) == 1) {
				throw new NotSupportedException("Encrypted Zip files are not supported");
			}

			memoryBuffer = new RentedArray<byte>(checked((int) memorySize));
			// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
			switch (compression) {
				case ZipCompression.Deflate:
				case ZipCompression.Deflate64: {
					CompressionHelper.Decompress(CompressionType.Deflate, diskBuffer.Memory, memoryBuffer.Memory);
					return memoryBuffer;
				}
				default:
					throw new UnreachableException();
			}
		} catch {
			memoryBuffer?.Dispose();
			throw;
		} finally {
			diskBuffer.Dispose();
		}
	}
}
