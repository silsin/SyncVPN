/*
 * Copyright (c) 2023 Proton AG
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

using SyncVPN.Configurations.Contracts.Entities;

namespace SyncVPN.Configurations.Entities;

public class OpenVpnConfigurations : IOpenVpnConfigurations
{
    public string ConfigPath { get; set; } = string.Empty;

    public string TapAdapterId { get; set; } = string.Empty;
    public string TapAdapterDescription { get; set; } = string.Empty;
    public string TapInstallerDir { get; set; } = string.Empty;

    public string TunAdapterId { get; set; } = string.Empty;
    public string TunAdapterName { get; set; } = string.Empty;

    public string TlsExportCertFolder { get; set; } = string.Empty;
    public string ExePath { get; set; } = string.Empty;
    public string TlsVerifyExePath { get; set; } = string.Empty;

    public string ManagementHost { get; set; } = string.Empty;
    public string ExitEventName { get; set; } = string.Empty;
    public byte[] StaticKey { get; set; } = Array.Empty<byte>();
}