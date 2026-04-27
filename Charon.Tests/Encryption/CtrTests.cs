// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Security.Cryptography;
using Charon.Encryption.Transform;

namespace Charon.Tests.Encryption;

internal static class CtrTests {
	[Test]
	public static void DecryptTest() {
		var vec = new byte[] {
			0x0e, 0x4d, 0x7f, 0x40, 0xce, 0x83, 0xa1, 0x70, 0x41, 0xdf, 0x9b, 0x75, 0x00, 0x1b, 0xbb, 0x3d,
			0xd9, 0xd4, 0x95, 0x7b, 0x5b, 0xac, 0xc3, 0xe3, 0x65, 0xdc, 0xdf, 0x93, 0xc4, 0x8c, 0xc4, 0x59,
		};
		var exp = "__Neptune/CharonNeptune/Charon__"u8.ToArray();
		var key = "_CHARON__CHARON_"u8.ToArray();
		var iv = "_NEPTUNENEPTUNE_"u8.ToArray();

		using var aes = Aes.Create();
		aes.Key = key;
		aes.IV = iv;
		CtrTransform.Crypt(aes, vec);
		Assert.That(vec, Is.EqualTo(exp).AsCollection);
	}

	[Test]
	public static void EncryptTest() {
		var exp = new byte[] {
			0x0e, 0x4d, 0x7f, 0x40, 0xce, 0x83, 0xa1, 0x70, 0x41, 0xdf, 0x9b, 0x75, 0x00, 0x1b, 0xbb, 0x3d,
			0xd9, 0xd4, 0x95, 0x7b, 0x5b, 0xac, 0xc3, 0xe3, 0x65, 0xdc, 0xdf, 0x93, 0xc4, 0x8c, 0xc4, 0x59,
		};
		var vec = "__Neptune/CharonNeptune/Charon__"u8.ToArray();
		var key = "_CHARON__CHARON_"u8.ToArray();
		var iv = "_NEPTUNENEPTUNE_"u8.ToArray();

		using var aes = Aes.Create();
		aes.Key = key;
		aes.IV = iv;
		CtrTransform.Crypt(aes, vec);
		Assert.That(vec, Is.EqualTo(exp).AsCollection);
	}

	[Test]
	public static void DecryptPartialTest() {
		var vec = new byte[] {
			0x1f, 0x77, 0x41, 0x51, 0xcb, 0x99, 0xb1, 0x31, 0x67, 0x98, 0xb9, 0x6f, 0x0e, 0x07, 0x9a, 0x36,
			0xe7, 0xc5, 0x90, 0x61, 0x4b, 0xed, 0xe5, 0xa4, 0x47, 0xc6, 0xd1, 0x8f,
		};
		var exp = "Neptune/CharonNeptune/Charon"u8.ToArray();
		var key = "_CHARON__CHARON_"u8.ToArray();
		var iv = "_NEPTUNENEPTUNE_"u8.ToArray();

		using var aes = Aes.Create();
		aes.Key = key;
		aes.IV = iv;
		CtrTransform.Crypt(aes, vec);
		Assert.That(vec, Is.EqualTo(exp).AsCollection);
	}

	[Test]
	public static void EncryptPartialTest() {
		var exp = new byte[] {
			0x1f, 0x77, 0x41, 0x51, 0xcb, 0x99, 0xb1, 0x31, 0x67, 0x98, 0xb9, 0x6f, 0x0e, 0x07, 0x9a, 0x36,
			0xe7, 0xc5, 0x90, 0x61, 0x4b, 0xed, 0xe5, 0xa4, 0x47, 0xc6, 0xd1, 0x8f,
		};
		var vec = "Neptune/CharonNeptune/Charon"u8.ToArray();
		var key = "_CHARON__CHARON_"u8.ToArray();
		var iv = "_NEPTUNENEPTUNE_"u8.ToArray();

		using var aes = Aes.Create();
		aes.Key = key;
		aes.IV = iv;
		CtrTransform.Crypt(aes, vec);
		Assert.That(vec, Is.EqualTo(exp).AsCollection);
	}
}
