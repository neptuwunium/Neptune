// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Triton.Pixel.Channels;

public interface IChannel {
	static virtual bool IsRed => false;
	static virtual bool IsGreen => false;
	static virtual bool IsBlue => false;
	static virtual bool IsAlpha => false;
	static virtual bool FullyUtilized => true;

	static virtual T GetBlack<T>() where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => NumericConversion.GetMinimumValueSafe<T>();
	static virtual T GetWhite<T>() where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => NumericConversion.GetMaximumValueSafe<T>();
	static virtual T GetTransparent<T>() where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => NumericConversion.GetMinimumValueSafe<T>();
	static virtual T GetRed<T>(T field) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field;
	static virtual T GetGreen<T>(T field) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field;
	static virtual T GetBlue<T>(T field) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field;
	static virtual T GetAlpha<T>(T field) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field;
	static virtual void SetRed<T>(ref T field, T value) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field = value;
	static virtual void SetGreen<T>(ref T field, T value) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field = value;
	static virtual void SetBlue<T>(ref T field, T value) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field = value;
	static virtual void SetAlpha<T>(ref T field, T value) where T : unmanaged, INumberBase<T>, IMinMaxValue<T> => field = value;
}
