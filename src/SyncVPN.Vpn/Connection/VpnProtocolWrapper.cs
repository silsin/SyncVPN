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

using System;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Common.Core.Networking;
using SyncVPN.Common.Legacy;
using SyncVPN.Common.Legacy.Vpn;
using SyncVPN.Vpn.Common;

namespace SyncVPN.Vpn.Connection;

internal class VpnProtocolWrapper : ISingleVpnConnection
{
    private readonly ISingleVpnConnection _openVpnConnection;
    private readonly ISingleVpnConnection _wireGuardConnection;
    private readonly ISingleVpnConnection _l2tpConnection;
    private readonly ISingleVpnConnection _sstpConnection;

    private VpnProtocol _vpnProtocol;

    public VpnProtocolWrapper(ISingleVpnConnection openVpnConnection,
        ISingleVpnConnection wireGuardConnection,
        ISingleVpnConnection l2tpConnection,
        ISingleVpnConnection sstpConnection)
    {
        _openVpnConnection = openVpnConnection;
        _wireGuardConnection = wireGuardConnection;
        _l2tpConnection = l2tpConnection;
        _sstpConnection = sstpConnection;

        _openVpnConnection.StateChanged += OnStateChanged;
        _wireGuardConnection.StateChanged += OnStateChanged;
        _l2tpConnection.StateChanged += OnStateChanged;
        _sstpConnection.StateChanged += OnStateChanged;
    }

    public event EventHandler<EventArgs<VpnState>> StateChanged;
    public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
    {
        add
        {
            _openVpnConnection.ConnectionDetailsChanged += value;
            _wireGuardConnection.ConnectionDetailsChanged += value;
            _l2tpConnection.ConnectionDetailsChanged += value;
            _sstpConnection.ConnectionDetailsChanged += value;
        }
        remove
        {
            _openVpnConnection.ConnectionDetailsChanged -= value;
            _wireGuardConnection.ConnectionDetailsChanged -= value;
            _l2tpConnection.ConnectionDetailsChanged -= value;
            _sstpConnection.ConnectionDetailsChanged -= value;
        }
    }

    public NetworkTraffic NetworkTraffic => VpnConnection?.NetworkTraffic ?? NetworkTraffic.Zero;

    public void Connect(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
    {
        _vpnProtocol = config.VpnProtocol;
        VpnConnection.Connect(endpoint, credentials, config);
    }

    public void Disconnect(VpnError error)
    {
        if (VpnConnection == null)
        {
            _openVpnConnection.Disconnect(error);
            _wireGuardConnection.Disconnect(error);
            _l2tpConnection.Disconnect(error);
            _sstpConnection.Disconnect(error);
            OnStateChanged(this, new EventArgs<VpnState>(new VpnState(VpnStatus.Disconnected, _vpnProtocol)));
        }
        else
        {
            VpnConnection.Disconnect(error);
        }
    }

    public void SetFeatures(VpnFeatures vpnFeatures)
    {
        VpnConnection?.SetFeatures(vpnFeatures);
    }

    public void RequestNetShieldStats()
    {
        VpnConnection?.RequestNetShieldStats();
    }

    public void RequestConnectionDetails()
    {
        VpnConnection?.RequestConnectionDetails();
    }

    private void OnStateChanged(object sender, EventArgs<VpnState> e)
    {
        StateChanged?.Invoke(this, e);
    }

    private ISingleVpnConnection VpnConnection => _vpnProtocol switch
    {
        _ when _vpnProtocol.IsWireGuard() => _wireGuardConnection,
        _ when _vpnProtocol.IsOpenVpn() => _openVpnConnection,
        VpnProtocol.L2tp => _l2tpConnection,
        VpnProtocol.Sstp => _sstpConnection,
        _ => null,
    };
}