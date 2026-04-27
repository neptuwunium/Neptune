// SPDX-FileCopyrightText: 2026 Neptuwunium
//
// SPDX-License-Identifier: Zlib

using System.Reflection;
using System.Runtime.InteropServices;

namespace SDL;

public static partial class SDL3ShaderCross {
	static SDL3ShaderCross() => NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);

	internal static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) {
		if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var handle)) {
			return handle;
		}

		var name = Path.GetFileNameWithoutExtension(libraryName);
		var cwd = AppDomain.CurrentDomain.BaseDirectory;

		string ext;
		if (OperatingSystem.IsWindows()) {
			ext = ".dll";
		} else if (OperatingSystem.IsLinux()) {
			ext = ".so";
		} else if (OperatingSystem.IsMacOS()) {
			ext = ".dylib";
		} else {
			return nint.Zero;
		}

		foreach (var dir in new[] { Path.Combine(cwd, $"runtimes/{RuntimeInformation.RuntimeIdentifier}/native/"), cwd }) {
			foreach (var libName in new[] { name, "lib" + name }) {
				var target = Path.Combine(dir, libName) + ext;
				if (!File.Exists(target)) {
					continue;
				}

				var ptr = NativeLibrary.Load(target);
				if (ptr != nint.Zero) {
					return ptr;
				}
			}
		}

		return nint.Zero;
	}
}
