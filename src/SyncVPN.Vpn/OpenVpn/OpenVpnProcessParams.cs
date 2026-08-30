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

using System.Collections.Generic;
using SyncVPN.Common.Legacy;
using SyncVPN.Common.Core.Networking;
using SyncVPN.Vpn.Common;

namespace SyncVPN.Vpn.OpenVpn;

public class OpenVpnProcessParams
{
    public OpenVpnProcessParams(
        VpnEndpoint endpoint,
        int managementPort,
        string password,
        IReadOnlyCollection<string> customDns,
        SplitTunnelMode splitTunnelMode,
        OpenVpnAdapter openVpnAdapter,
        string interfaceGuid)
    {
        Endpoint = endpoint;
        ManagementPort = managementPort;
        Password = password;
        CustomDns = customDns;
        SplitTunnelMode = splitTunnelMode;
        OpenVpnAdapter = openVpnAdapter;
        InterfaceGuid = interfaceGuid;
    }

    public VpnEndpoint Endpoint { get; }

    public int ManagementPort { get; }

    public string Password { get; }

    public IReadOnlyCollection<string> CustomDns { get; }

    public SplitTunnelMode SplitTunnelMode { get; }

    public IReadOnlyCollection<string> SplitTunnelIPs { get; }

    public OpenVpnAdapter OpenVpnAdapter { get; }

    public string InterfaceGuid { get; }
}