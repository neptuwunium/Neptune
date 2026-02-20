using System.Reflection;
using System.Runtime.InteropServices;

namespace Triton;

internal static class NativeHelper {
	private static bool Initialized { get; set; }

	public static void Register() {
		if (!Initialized) {
			NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
			Initialized = true;
		}
	}

	public static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) {
		if (NativeLibrary.TryLoad(libraryName, assembly, searchPath, out var handle)) {
			return handle;
		}

		if (searchPath != null && (searchPath & DllImportSearchPath.AssemblyDirectory) == 0) {
			return nint.Zero;
		}

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

		var name = Path.GetFileNameWithoutExtension(libraryName);
		var cwd = AppDomain.CurrentDomain.BaseDirectory;

		foreach (var dir in new[] { Path.Combine(cwd, $"runtimes/{RuntimeInformation.RuntimeIdentifier}/native/"), cwd }) {
			foreach (var prefix in new[] { "lib", "" }) {
				var target = Path.Combine(dir, prefix + name) + ext;
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
