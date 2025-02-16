// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

namespace Triton.Pixel.Channels;

public readonly record struct A : IChannel {
	static bool IChannel.IsAlpha => true;
	static T IChannel.GetBlack<T>() => NumericConversion.GetMaximumValueSafe<T>();
	static T IChannel.GetWhite<T>() => NumericConversion.GetMinimumValueSafe<T>();
}
