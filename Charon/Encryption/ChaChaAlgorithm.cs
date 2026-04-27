// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Numerics;
using Pluto.Extensions;

namespace Charon.Encryption;

// todo: conform to SymmetricAlgorithm
public class ChaChaAlgorithm {
	public uint[] Keystream = new uint[16];
	public uint[] State = new uint[16];

	public ChaChaAlgorithm(ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, ReadOnlySpan<byte> stateKey, uint counter) {
		key.CopyTo(Key);
		iv.CopyTo(IV);

		var key32 = MemoryMarshal.Cast<byte, uint>(key);
		var iv32 = MemoryMarshal.Cast<byte, uint>(iv);
		var stateKey32 = MemoryMarshal.Cast<byte, uint>(stateKey);

		State[0] = stateKey32[0];
		State[1] = stateKey32[1];
		State[2] = stateKey32[2];
		State[3] = stateKey32[3];
		State[4] = key32[0];
		State[5] = key32[1];
		State[6] = key32[2];
		State[7] = key32[3];
		if (key32.Length >= 8) {
			State[8] = key32[4];
			State[9] = key32[5];
			State[10] = key32[6];
			State[11] = key32[7];
		}

		State[12] = counter;
		State[13] = iv32[0];
		State[14] = iv32[1];
		if (iv32.Length >= 3) {
			State[15] = iv32[2];
		}

		Position = 64;
	}

	public int Position { get; set; }
	public byte[] Key { get; set; } = new byte[32];
	public byte[] IV { get; set; } = new byte[16];
	public static ReadOnlySpan<byte> DefaultState => "expand 32-byte k"u8.AsBytes();

	private void Round(int a, int b, int c, int d) {
		Keystream[a] += Keystream[b];
		Keystream[d] = BitOperations.RotateLeft(Keystream[d] ^ Keystream[a], 16);
		Keystream[c] += Keystream[d];
		Keystream[b] = BitOperations.RotateLeft(Keystream[b] ^ Keystream[c], 12);
		Keystream[a] += Keystream[b];
		Keystream[d] = BitOperations.RotateLeft(Keystream[d] ^ Keystream[a], 8);
		Keystream[c] += Keystream[d];
		Keystream[b] = BitOperations.RotateLeft(Keystream[b] ^ Keystream[c], 7);
	}

	public void NextBlock(ReadOnlySpan<uint> lastBlock, int rounds) {
		if (lastBlock.Length > 0) {
			for (var i = 0; i < 16; i++) {
				Keystream[i] = lastBlock[i] ^ State[i];
			}
		} else {
			for (var i = 0; i < 16; i++) {
				Keystream[i] = State[i];
			}
		}

		for (var i = rounds; i > 0; i -= 2) {
			Round(0, 4, 8, 12);
			Round(1, 5, 9, 13);
			Round(2, 6, 10, 14);
			Round(3, 7, 11, 15);
			Round(0, 5, 10, 15);
			Round(1, 6, 11, 12);
			Round(2, 7, 8, 13);
			Round(3, 4, 9, 14);
		}

		if (lastBlock.Length > 0) {
			for (var i = 0; i < 16; i++) {
				Keystream[i] += lastBlock[i] ^ State[i];
			}
		} else {
			for (var i = 0; i < 16; i++) {
				Keystream[i] += State[i];
			}
		}

		State[12]++;
		if (State[12] == 0) {
			State[13]++;
		}

		Position = 0;
	}

	public void CryptBytes(ref Span<byte> bytes, int offset, int length, int rounds = 20, bool reverse = false) {
		var keystream8 = MemoryMarshal.Cast<uint, byte>(Keystream.AsSpan());
		var slice = bytes[offset..];
		for (var i = 0; i < length; i++) {
			if (Position >= 64) {
				NextBlock(default, rounds);
			}

			slice[reverse ? length - i - 1 : i] ^= keystream8[Position++];
		}
	}

	public void CryptStream(ref Span<byte> bytes, int offset, int length, Span<int> rounds = default, bool reverse = false) {
		var keystream8 = MemoryMarshal.Cast<uint, byte>(Keystream.AsSpan());
		var slice = bytes[offset..];
		var block = 0;
		for (var i = 0; i < length; i++) {
			if (Position >= 64) {
				var lastBlock = default(Span<uint>);
				if (block > 0) {
					lastBlock = Keystream.AsSpan().Clone();
				}

				NextBlock(lastBlock, rounds.Length == 0 ? 8 : rounds[block++ % rounds.Length]);
			}

			if (reverse) {
				slice[^i] ^= keystream8[Position++];
			} else {
				slice[i] ^= keystream8[Position++];
			}
		}
	}
}
