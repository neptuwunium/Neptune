// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Charon.Encryption;

namespace Charon.Tests.Encryption;

internal static class RijndaelTests {
	[TestCase("_CHARON__CHARON_", "6541b1f166de7cc40da76c191e99e323")]
	[TestCase("_CHARON__CHARON__CHARON_", "8ebe0595878743ceca46c5ac520a1c17")]
	[TestCase("_CHARON__CHARON__CHARON__CHARON_", "d2c2b39a719ecdeaf456df3d7419df24")]
	public static void DecryptTest(string keyText, string vecText) {
		var vec = Convert.FromHexString(vecText);
		var exp = "_Neptune/Charon_"u8.ToArray();
		var key = Encoding.ASCII.GetBytes(keyText);

		var rk = RaccoonRijndael.CreateDecryptRoundKey(key, key.Length * 8);
		RaccoonRijndael.DecryptRound(rk, vec, key.Length * 8);
		Assert.That(vec, Is.EqualTo(exp).AsCollection);
	}

	[TestCase("_CHARON__CHARON_", "6541b1f166de7cc40da76c191e99e323")]
	[TestCase("_CHARON__CHARON__CHARON_", "8ebe0595878743ceca46c5ac520a1c17")]
	[TestCase("_CHARON__CHARON__CHARON__CHARON_", "d2c2b39a719ecdeaf456df3d7419df24")]
	public static void EncryptTest(string keyText, string vecText) {
		var exp = Convert.FromHexString(vecText);
		var vec = "_Neptune/Charon_"u8.ToArray();
		var key = Encoding.ASCII.GetBytes(keyText);

		var rk = RaccoonRijndael.CreateEncryptRoundKey(key, key.Length * 8);
		RaccoonRijndael.EncryptRound(rk, vec, key.Length * 8);
		Assert.That(vec, Is.EqualTo(exp).AsCollection);
	}
}
