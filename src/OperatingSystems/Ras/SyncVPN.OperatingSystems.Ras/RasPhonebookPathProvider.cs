/*
 * Copyright (c) 2026 Proton AG
 *
 * This file is part of SyncVPN.
 *
 * SyncVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * SyncVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with SyncVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.IO;

namespace SyncVPN.OperatingSystems.Ras;

// A service-owned phonebook (.pbk) file, separate from any user's default phonebook, so entries
// created here are never visible in Windows' own "VPN" settings UI and can't collide with a user's
// own connections. RasSetEntryProperties creates the file automatically if it doesn't exist, but the
// containing directory must already exist.
internal static class RasPhonebookPathProvider
{
    private static readonly string _phonebookPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "SyncVPN", "Ras", "syncvpn.pbk");

    public static string GetPath()
    {
        string? directory = Path.GetDirectoryName(_phonebookPath);
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return _phonebookPath;
    }
}
