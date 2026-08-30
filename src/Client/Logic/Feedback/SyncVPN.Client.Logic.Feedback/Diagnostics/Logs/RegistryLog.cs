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

using System.Text;
using Microsoft.Win32;
using SyncVPN.Configurations.Contracts;
using SyncVPN.OperatingSystems.Registries.Contracts;

namespace SyncVPN.Client.Logic.Feedback.Diagnostics.Logs;

public class RegistryLog : LogBase
{
    private readonly IStaticConfiguration _config;
    private readonly IRegistryEditor _registryEditor;

    public RegistryLog(
        IStaticConfiguration config,
        IRegistryEditor registryEditor)
        : base(config.DiagnosticLogsFolder, "Registry.txt")
    {
        _config = config;
        _registryEditor = registryEditor;
    }

    public override void Write()
    {
        File.WriteAllText(Path, GenerateContent());
    }

    private string GenerateContent()
    {
        StringBuilder stringBuilder = new();

        stringBuilder
            .AppendLine("============================================")
            .AppendLine(" REGISTRY REPORT")
            .AppendLine("============================================")
            .AppendLine();

        AppendStartupActivationRegistryEntries(stringBuilder);
        AppendProtocolActivationRegistryEntries(stringBuilder);
        AppendServiceRegistryEntries(stringBuilder);

        return stringBuilder.ToString();
    }

    private void AppendStartupActivationRegistryEntries(StringBuilder stringBuilder)
    {
        stringBuilder
            .AppendLine("--------------------------------------------")
            .AppendLine(" Startup Activation (Auto Launch)")
            .AppendLine("--------------------------------------------")
            .AppendLine();

        AppendRegistryEntries(stringBuilder, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run");
        AppendRegistryEntries(stringBuilder, RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run");
    }

    private void AppendProtocolActivationRegistryEntries(StringBuilder stringBuilder)
    {
        stringBuilder
            .AppendLine("--------------------------------------------")
            .AppendLine(" Protocol Activation (Deeplink)")
            .AppendLine("--------------------------------------------")
            .AppendLine();

        AppendRegistryEntries(stringBuilder, RegistryHive.ClassesRoot, @"protonvpn");
        AppendRegistryEntries(stringBuilder, RegistryHive.ClassesRoot, @"proton-vpn");
        AppendRegistryEntries(stringBuilder, RegistryHive.CurrentUser, @"Software\Classes\protonvpn");
        AppendRegistryEntries(stringBuilder, RegistryHive.CurrentUser, @"Software\Classes\proton-vpn");
    }

    private void AppendServiceRegistryEntries(StringBuilder stringBuilder)
    {
        stringBuilder
            .AppendLine("--------------------------------------------")
            .AppendLine(" Services")
            .AppendLine("--------------------------------------------")
            .AppendLine();

        AppendRegistryEntries(stringBuilder, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SyncVPN Service");
        AppendRegistryEntries(stringBuilder, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SyncVPN WireGuard");
        AppendRegistryEntries(stringBuilder, RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\SyncVPNCallout");
    }

    private void AppendRegistryEntries(StringBuilder stringBuilder, RegistryHive hive, string path)
    {
        try
        {
            stringBuilder
                .AppendLine($"PATH: {hive}\\{path}")
                .AppendLine();

            RegistryUri uri = hive switch
            {
                RegistryHive.CurrentUser => RegistryUri.CreateCurrentUserUri(path, string.Empty),
                RegistryHive.LocalMachine => RegistryUri.CreateLocalMachineUri(path, string.Empty),
                RegistryHive.ClassesRoot => RegistryUri.CreateClassesRootUri(path, string.Empty),
                _ => throw new NotSupportedException($"Registry hive {hive} is not supported.")
            };

            Dictionary<string, string> entries = _registryEditor.ReadAll(uri);

            if (entries.Count > 0)
            {
                foreach (KeyValuePair<string, string> entry in entries)
                {
                    string key = string.IsNullOrEmpty(entry.Key) ? "(empty)" : entry.Key;
                    string value = string.IsNullOrEmpty(entry.Value) ? "(empty)" : entry.Value;
                    stringBuilder.AppendLine($"- {key}: {value}");
                }
            }
            else
            {
                stringBuilder.AppendLine("- (No entries found)");
            }
        }
        catch (Exception e)
        {
            stringBuilder.AppendLine($"- (Error: {e.Message})");
        }

        stringBuilder.AppendLine();
    }
}