// SPDX-FileCopyrightText: 2024-2026 Neptuwunium
//
// SPDX-License-Identifier: EUPL-1.2

namespace Pluto.Platform;

public static class PlatformUtils {
	public static bool CanCreateSymlinks => !OperatingSystem.IsWindows() || WindowsPlatform.HasSECreateSymbolicLinkPrivilege();
}
