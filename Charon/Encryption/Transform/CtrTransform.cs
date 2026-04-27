// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Security.Cryptography;

namespace Charon.Encryption.Transform;

public static class CtrTransform {
	public static unsafe Span<byte> Crypt(ICryptoTransform transform, byte[] iv, Span<byte> input) {
		var ctr = (byte[]) iv.Clone();
		var blockSize = ctr.Length;
		var counterModeBlock = new byte[blockSize];

		fixed (byte* datap = &input.GetPinnableReference()) {
			fixed (byte* ctrp = &counterModeBlock[0]) {
				for (var i = 0; i < input.Length / blockSize; i++) {
					RotateCtr();
					XorUtils.XorUnsafe(datap + i * blockSize, blockSize, ctrp, blockSize);
				}
			}
		}

		// final block
		RotateCtr();

		var offset = input.Length - input.Length % blockSize;
		if (offset == input.Length) {
			return input;
		}

		var tail = input[offset..];
		for (var i = 0; i < tail.Length; i++) {
			tail[i] = (byte) (tail[i] ^ counterModeBlock[i]);
		}

		return input;

		void RotateCtr() {
			transform.TransformBlock(ctr, 0, blockSize, counterModeBlock, 0);
			for (var j = ctr.Length - 1; j >= 0; j--) {
				if (++ctr[j] != 0) {
					break;
				}
			}
		}
	}

	public static Span<byte> Crypt(SymmetricAlgorithm algorithm, Span<byte> input) {
		algorithm.Mode = CipherMode.ECB;
		algorithm.Padding = PaddingMode.None;

		var blockSize = algorithm.BlockSize / 8;

		var iv = algorithm.IV;
		algorithm.IV = new byte[blockSize];

		Crypt(algorithm.CreateEncryptor(), iv, input);

		return input;
	}
}
