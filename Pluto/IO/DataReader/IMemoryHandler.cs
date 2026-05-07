// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pluto.IO.DataReader;

public interface IMemoryHandler : IDisposable {
	static IMemoryHandler Handler { get; set; } = default!;
	bool ReadBytes(nint address, Span<byte> buffer, out int bytesRead);
	IEnumerable<(nint ModuleStart, int ModuleSize, string ModuleName)> EnumerateModules();
	Dictionary<string, List<(nint ModuleStart, int ModuleSize, SectionFlags Flags)>> MapSections();
}

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 8)]
public record struct MemoryStrPtr<T>(nint Address) where T : unmanaged {
	public MemoryStrPtr<TOther> As<TOther>() where TOther : unmanaged => new(Address);

	public string? Value => TryReadStr(out var value) ? value : default;

	public bool TryReadStr([MaybeNullWhen(false)] out string value) => TryReadStr(0x800, Encoding.UTF8, out value);
	public bool TryReadStr(Encoding encoding, [MaybeNullWhen(false)] out string value) => TryReadStr(0x800, encoding, out value);
	public bool TryReadStr(int bufferSize, [MaybeNullWhen(false)] out string value) => TryReadStr(bufferSize, Encoding.UTF8, out value);

	public bool TryReadStr(int bufferSize, Encoding encoding, [MaybeNullWhen(false)] out string value) {
		value = null;
		if (Address == nint.Zero) {
			return false;
		}

		var slop = (stackalloc T[bufferSize]);
		if (!IMemoryHandler.Handler.ReadBytes(Address, MemoryMarshal.AsBytes(slop), out var read)) {
			return false;
		}

		var index = slop[..read].IndexOf(default(T));
		if (index == -1) {
			index = read;
		}

		value = encoding.GetString(MemoryMarshal.AsBytes(slop[..index]));
		return true;
	}

	public bool IsNull => Address == nint.Zero;
	public override string ToString() => Value ?? $"@{Address.ToString("x016")}";
	public static implicit operator string(MemoryStrPtr<T> value) => value.Value ?? string.Empty;
}

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 8)]
public record struct MemoryPtr<T>(nint Address) where T : unmanaged {
	public MemoryPtr<TOther> As<TOther>() where TOther : unmanaged => new(Address);

	public bool TryRead(out T value) => TryRead(0, out value);

	public bool TryRead(int index, out T value) {
		value = default;

		if (Address == nint.Zero || IMemoryHandler.Handler is not { } handler) {
			return false;
		}

		var address = Address + Unsafe.SizeOf<T>() * index;

		var refVal = default(T);
		if (!handler.ReadBytes(address, MemoryMarshal.AsBytes(new Span<T>(ref refVal)), out var read) || read < Unsafe.SizeOf<T>()) {
			return false;
		}

		value = refVal;
		return true;
	}

	public bool IsNull => Address == nint.Zero;
	public T Value => TryRead(out var value) ? value : default;
	public override string ToString() => Address.ToString("x016");
	public static implicit operator T(MemoryPtr<T> value) => value.Value;
}
