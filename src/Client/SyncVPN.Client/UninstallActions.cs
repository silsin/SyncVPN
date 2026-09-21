/*
 * Copyright (c) 2025 Proton AG
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

using Microsoft.Win32;
using SyncVPN.Configurations.Defaults;

namespace SyncVPN.Client;

public static class UninstallActions
{
    /// <summary>
    /// The method executed by the uninstaller without administrative privileges
    /// </summary>
    public static void DeleteClientData()
    {
        try
        {
            Directory.Delete(DefaultConfiguration.LocalAppDataPath, true);
        }
        catch
        {
        }
    }

    /// <summary>
    /// Deletes registry keys and values associated with SyncVPN startup configuration.
    /// </summary>
    /// <remarks>
    /// <para>The following registry keys and values will be deleted:</para>
    /// <para>- HKCU\Software\Classes\protonvpn (legacy scheme)</para>
    /// <para>- HKCU\Software\Classes\sync-vpn</para>
    /// <para>- HKCU\Software\Classes\AppUserModelId\{appUserModelId}</para>
    /// <para>- HKCU\Software\Microsoft\Windows\CurrentVersion\Run (Value name: {appName})</para>
    /// <para>- HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run (Value name: {appName})</para>
    /// </remarks>
    public static void DeleteRegistryKeys(string appUserModelId, string appName)
    {
        try
        {
            DeleteSubKeyTree($"Software\\Classes\\AppUserModelId\\{appUserModelId}");
            DeleteSubKeyTree($"Software\\Classes\\{DefaultConfiguration.ProtocolActivationScheme}");
            DeleteSubKeyTree($"Software\\Classes\\{DefaultConfiguration.LegacyProtocolActivationScheme}");

            DeleteValue(@"Software\Microsoft\Windows\CurrentVersion\Run", appName);
            DeleteValue(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", appName);
        }
        catch (Exception)
        {
        }
    }

    private static void DeleteSubKeyTree(string keyPath)
    {
        using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath))
        {
            if (key != null)
            {
                Registry.CurrentUser.DeleteSubKeyTree(keyPath);
            }
        }
    }

    private static void DeleteValue(string subKey, string name)
    {
        using (RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(subKey, true))
        {
            if (registryKey != null && registryKey.GetValue(name) != null)
            {
                registryKey.DeleteValue(name);
            }
        }
    }
}