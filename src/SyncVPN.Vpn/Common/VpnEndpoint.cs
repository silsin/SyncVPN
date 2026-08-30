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

namespace SyncVPN.Vpn.Common;

public class VpnEndpoint
{
    public VpnProtocol VpnProtocol { get; }

    public VpnHost Server { get; }
    public int Port { get; }
    public bool IsEmpty { get; }

    public VpnEndpoint(VpnHost server, VpnProtocol vpnProtocol)
        : this(server, vpnProtocol, 0)
    {
    }

    public VpnEndpoint(VpnHost server, int port, bool isEmpty = false)
    {
        Server = server;
        Port = port;
        IsEmpty = isEmpty;
    }

    public VpnEndpoint(VpnHost server, VpnProtocol vpnProtocol, int port)
        : this(server, port)
    {
        VpnProtocol = vpnProtocol;
    }

    public VpnEndpoint() : this(default, default, true)
    {
    }

    public static VpnEndpoint Empty => new();
}