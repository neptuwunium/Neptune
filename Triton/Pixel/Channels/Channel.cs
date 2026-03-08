// SPDX-FileCopyrightText: 2022 - 2026 ds5678
// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Triton.Pixel.Channels;

public static class Channel {
	public static bool TryGetRed<TChannel, TValue>(in TValue field, out TValue value)
		where TChannel : IChannel
		where TValue : unmanaged, INumberBase<TValue>, IMinMaxValue<TValue> {
		if (TChannel.IsRed) {
			value = TChannel.GetRed(field);
			return true;
		}

		value = default;
		return false;
	}

	public static bool TryGetGreen<TChannel, TValue>(in TValue field, out TValue value)
		where TChannel : IChannel
		where TValue : unmanaged, INumberBase<TValue>, IMinMaxValue<TValue> {
		if (TChannel.IsGreen) {
			value = TChannel.GetGreen(field);
			return true;
		}

		value = default;
		return false;
	}

	public static bool TryGetBlue<TChannel, TValue>(in TValue field, out TValue value)
		where TChannel : IChannel
		where TValue : unmanaged, INumberBase<TValue>, IMinMaxValue<TValue> {
		if (TChannel.IsBlue) {
			value = TChannel.GetBlue(field);
			return true;
		}

		value = default;
		return false;
	}

	public static bool TryGetAlpha<TChannel, TValue>(in TValue field, out TValue value)
		where TChannel : IChannel
		where TValue : unmanaged, INumberBase<TValue>, IMinMaxValue<TValue> {
		if (TChannel.IsAlpha) {
			value = TChannel.GetAlpha(field);
			return true;
		}

		value = default;
		return false;
	}

	public static void SetIfRed<TChannel, TValue>(ref TValue field, TValue value)
		where TChannel : IChannel
		where TValue : unmanaged, INumberBase<TValue>, IMinMaxValue<TValue> {
		if (TChannel.IsRed) {
			TChannel.SetRed(ref field, value);
		}
	}

	public static void SetIfGreen<TChannel, TValue>(ref TValue field, TValue value)
		where TChannel : IChannel
		where TValue : unmanaged, INumberBase<TValue>, IMinMaxValue<TValue> {
		if (TChannel.IsGreen) {
			TChannel.SetGreen(ref field, value);
		}
	}

	public static void SetIfBlue<TChannel, TValue>(ref TValue field, TValue value)
		where TChannel : IChannel
		where TValue : unmanaged, INumberBase<TValue>, IMinMaxValue<TValue> {
		if (TChannel.IsBlue) {
			TChannel.SetBlue(ref field, value);
		}
	}

	public static void SetIfAlpha<TChannel, TValue>(ref TValue field, TValue value)
		where TChannel : IChannel
		where TValue : unmanaged, INumberBase<TValue>, IMinMaxValue<TValue> {
		if (TChannel.IsAlpha) {
			TChannel.SetAlpha(ref field, value);
		}
	}
}
