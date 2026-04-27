// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Charon.Hash.Algorithms;

namespace Charon.Tests.Hash;

internal static class MD4Tests {
	[TestCase("", "31d6cfe0d16ae931b73c59d7e0c089c0")]
	[TestCase("a", "bde52cb31de33e46245e05fbdbd6fb24")]
	[TestCase("abc", "a448017aaf21d8525fc10ae87aa6729d")]
	[TestCase("message digest", "d9130a8164549fe818874806e1c7014b")]
	[TestCase("abcdefghijklmnopqrstuvwxyz", "d79e1c308aa5bbcdeea8ed63df412da9")]
	[TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "043f8582f241db351ce627e153e7f0e4")]
	[TestCase("12345678901234567890123456789012345678901234567890123456789012345678901234567890", "e33b4ddc9c38f2199c3e7b164fcc0536")]
	public static void MD4Test(string input, string expect) {
		var md4 = new MD4Algorithm();
		md4.HashCore(Encoding.ASCII.GetBytes(input));
		var hash = (stackalloc byte[16]);
		md4.HashFinal(hash);
		Assert.That(Convert.ToHexString(hash).ToLowerInvariant(), Is.EqualTo(expect));
	}
}
