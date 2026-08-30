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

using SyncVPN.Common.Core.Networking;
using SyncVPN.Common.Legacy.Vpn;
using SyncVPN.Vpn.LocalAgent;

namespace SyncVPN.Vpn.Common;

public class VpnState
{
    public static VpnState Default => new(VpnStatus.Disconnected, VpnProtocol.Smart);

    public VpnStatus Status { get; }
    public VpnError Error { get; }
    public string LocalIp { get; }
    public string RemoteIp { get; }
    public int EndpointPort { get; }
    public OpenVpnAdapter? OpenVpnAdapter { get; }
    public VpnProtocol VpnProtocol { get; }
    public string Label { get; }
    public bool PortForwarding { get; }
    public ConnectionCertificate ConnectionCertificate { get; }

    public VpnState(VpnStatus status, VpnProtocol vpnProtocol)
        : this(status, VpnError.None, string.Empty, string.Empty, 0, vpnProtocol)
    {
    }

    public VpnState(VpnStatus status, VpnError error, VpnProtocol vpnProtocol)
        : this(status, error, string.Empty, string.Empty, 0, vpnProtocol)
    {
    }

    public VpnState(VpnStatus status, string remoteIp, int endpointPort, VpnProtocol vpnProtocol, OpenVpnAdapter? openVpnAdapter = null, string label = "")
    {
        Status = status;
        RemoteIp = remoteIp;
        EndpointPort = endpointPort;
        VpnProtocol = vpnProtocol;
        OpenVpnAdapter = openVpnAdapter;
        Label = label;
    }

    public VpnState(VpnStatus status, VpnError error, string localIp, string remoteIp, int endpointPort, VpnProtocol vpnProtocol,
        bool portForwarding = false, OpenVpnAdapter? openVpnAdapter = null, string label = "",
        ConnectionCertificate connectionCertificate = null)
    {
        Status = status;
        Error = error;
        LocalIp = localIp;
        RemoteIp = remoteIp;
        EndpointPort = endpointPort;
        VpnProtocol = vpnProtocol;
        PortForwarding = portForwarding;
        OpenVpnAdapter = openVpnAdapter;
        Label = label;
        ConnectionCertificate = connectionCertificate;
    }
}