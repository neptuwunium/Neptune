// SPDX-FileCopyrightText: 2022 - 2025 ds5678
// SPDX-FileCopyrightText: 2025 Legiayayana
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using System.Runtime.InteropServices;

namespace Triton.Pixel;

/// <summary>
///     An <see langword="interface" /> for handling color formats with up to 4 channels.
/// </summary>
/// <remarks>
///     When used as a generic type constraint, the methods and properties get devirtualized by the JIT compiler.
///     This prevents boxing when the implementing type is a <see langword="struct" />.
/// </remarks>
/// <typeparam name="TSelf">
///     The implementing type.
/// </typeparam>
/// <typeparam name="TChannelValue">
///     Supported types are:
///     <list type="bullet">
///         <item>
///             <see cref="sbyte" />
///         </item>
///         <item>
///             <see cref="byte" />
///         </item>
///         <item>
///             <see cref="short" />
///         </item>
///         <item>
///             <see cref="ushort" />
///         </item>
///         <item>
///             <see cref="int" />
///         </item>
///         <item>
///             <see cref="uint" />
///         </item>
///         <item>
///             <see cref="nint" />
///         </item>
///         <item>
///             <see cref="nuint" />
///         </item>
///         <item>
///             <see cref="long" />
///         </item>
///         <item>
///             <see cref="ulong" />
///         </item>
///         <item>
///             <see cref="Int128" />
///         </item>
///         <item>
///             <see cref="UInt128" />
///         </item>
///         <item>
///             <see cref="Half" />
///         </item>
///         <item>
///             <see cref="float" />
///         </item>
///         <item>
///             <see cref="NFloat" />
///         </item>
///         <item>
///             <see cref="double" />
///         </item>
///         <item>
///             <see cref="decimal" />
///         </item>
///     </list>
/// </typeparam>
public interface IColor<out TSelf, TChannelValue> : IColor
	where TSelf : unmanaged, IColor<TSelf, TChannelValue>
	where TChannelValue : unmanaged, INumberBase<TChannelValue> {
	/// <summary>
	///     The red channel
	/// </summary>
	TChannelValue R { get; set; }

	/// <summary>
	///     The green channel
	/// </summary>
	TChannelValue G { get; set; }

	/// <summary>
	///     The blue channel
	/// </summary>
	TChannelValue B { get; set; }

	/// <summary>
	///     The alpha channel
	/// </summary>
	TChannelValue A { get; set; }

	/// <summary>
	///     A black pixel.
	/// </summary>
	static abstract TSelf Black { get; }

	/// <summary>
	///     A white pixel.
	/// </summary>
	static abstract TSelf White { get; }

	/// <summary>
	///     A white pixel.
	/// </summary>
	static abstract TSelf Transparent { get; }

	/// <summary>
	///     Comparison Identifier for this color type.
	/// </summary>
	public static ColorId ColorId { get; } = ColorId.FromPixel<TSelf, TChannelValue>();

	void GetChannels(out TChannelValue r, out TChannelValue g, out TChannelValue b, out TChannelValue a);
	void SetChannels(TChannelValue r, TChannelValue g, TChannelValue b, TChannelValue a);
}
