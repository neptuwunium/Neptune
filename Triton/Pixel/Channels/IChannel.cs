// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Triton.Pixel.Channels;

public interface IChannel {
	public static virtual bool IsRed => false;
	public static virtual bool IsGreen => false;
	public static virtual bool IsBlue => false;
	public static virtual bool IsAlpha => false;
	public static virtual bool FullyUtilized => true;

	public static virtual T GetBlack<T>() where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => NumericConversion.GetMinimumValueSafe<T>();
	public static virtual T GetWhite<T>() where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => NumericConversion.GetMaximumValueSafe<T>();
	public static virtual T GetRed<T>(T field) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field;
	public static virtual T GetGreen<T>(T field) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field;
	public static virtual T GetBlue<T>(T field) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field;
	public static virtual T GetAlpha<T>(T field) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field;
	public static virtual void SetRed<T>(ref T field, T value) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field = value;
	public static virtual void SetGreen<T>(ref T field, T value) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field = value;
	public static virtual void SetBlue<T>(ref T field, T value) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field = value;
	public static virtual void SetAlpha<T>(ref T field, T value) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field = value;
}
