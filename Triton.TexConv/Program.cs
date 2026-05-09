// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using Pluto.IO.Binary;
using Pluto.IO.FileSystem;
using Triton.Encoder;
using Triton.Surface;

// todo: flags

if (!PNGEncoder.IsAvailable) {
	Console.WriteLine("error: png encoder is unavailable");
	return;
}

var encoder = new PNGEncoder(PNGCompressionLevel.Small);
var encoderOptions = new EncoderWriteOptions {
	AssociateAlpha = false,
	Compress = true,
};

foreach (var path in new FileEnumerator(args, new EnumerationOptions { RecurseSubdirectories = true })) {
	using var buffer = RentedArray<byte>.FromFile(path);

	if (buffer.Length <= 16) {
		continue;
	}

	if (MemoryMarshal.Read<uint>(buffer.Span) == 0x20534444) {
		Console.WriteLine(path);
		var dds = new DDS(buffer);

		// todo: special logic for cube-maps

		for (var index = 0; index < dds.ArrayCount; ++index) {
			try {
				using var surface = dds.GetSurface(index);
				if (surface == null) {
					Console.WriteLine($"error: cannot process surface {index}");
					continue;
				}

				var ext = index > 0 ? $".{index}.png" : ".png";
				var target = Path.ChangeExtension(path, ext);
				if (File.Exists(target)) {
					Console.WriteLine("warning: skipping as it already exists");
					continue;
				}

				using var stream = new FileStream(target, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
				encoder.Write(stream, encoderOptions, surface);
			} catch(Exception ex) {
				Console.WriteLine($"error: cannot process surface {index}\n{ex}");
			}
		}
	}
}
