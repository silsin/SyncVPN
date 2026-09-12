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
using SyncVPN.Common.Core.Networking;
using SyncVPN.Common.Legacy;
using SyncVPN.Common.Legacy.Vpn;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.ConnectLogs;
using SyncVPN.OperatingSystems.Ras.Contracts;
using SyncVPN.Vpn.Common;

namespace SyncVPN.Vpn.Connection;

// Dials a native Windows RAS L2TP/IPsec-PSK connection - see IRasConnection/RasConnection for the
// engine itself, and NativeMethods for the important caveat that engine is unverified against a real
// Windows SDK/server. Implements ISingleVpnConnection directly rather than IAdapterSingleVpnConnection:
// unlike OpenVpn/WireGuard, this isn't wrapped in LocalAgentWrapper, since Local Agent is Proton's
// proprietary post-connect control channel and has no equivalent for a plain RAS tunnel - so
// SetFeatures/RequestNetShieldStats/RequestConnectionDetails are no-ops here rather than being
// provided by that wrapper.
internal class L2tpConnection : ISingleVpnConnection
{
    private const string EntryName = "SyncVPN-L2TP";

    private readonly ILogger _logger;
    private readonly Func<IRasConnection> _rasConnectionFactory;

    private IRasConnection _rasConnection;
    private VpnProtocol _vpnProtocol;
    private VpnEndpoint _endpoint;

    public L2tpConnection(ILogger logger, Func<IRasConnection> rasConnectionFactory)
    {
        _logger = logger;
        _rasConnectionFactory = rasConnectionFactory;
    }

    public event EventHandler<EventArgs<VpnState>> StateChanged;
    public event EventHandler<ConnectionDetails> ConnectionDetailsChanged;

    public NetworkTraffic NetworkTraffic => NetworkTraffic.Zero;

    public void Connect(VpnEndpoint endpoint, VpnCredentials credentials, VpnConfig config)
    {
        _vpnProtocol = config.VpnProtocol;
        _endpoint = endpoint;

        if (string.IsNullOrEmpty(credentials.PreSharedKey))
        {
            _logger.Error<ConnectLog>("L2TP connection requested without a pre-shared key.");
            RaiseState(VpnStatus.Disconnected, VpnError.Unknown);
            return;
        }

        _rasConnection?.Dispose();
        _rasConnection = _rasConnectionFactory();
        _rasConnection.StateChanged += OnRasStateChanged;

        _rasConnection.Connect(new RasEntryOptions
        {
            EntryName = EntryName,
            Server = endpoint.Server.Ip,
            Username = credentials.Username,
            Password = credentials.Password,
            PreSharedKey = credentials.PreSharedKey,
            DeviceType = RasDeviceType.L2tp,
        });
    }

    public void Disconnect(VpnError error)
    {
        _rasConnection?.Disconnect();
    }

    public void SetFeatures(VpnFeatures vpnFeatures)
    {
    }

    public void RequestNetShieldStats()
    {
    }

    public void RequestConnectionDetails()
    {
    }

    private void OnRasStateChanged(object sender, RasStateChangedEventArgs e)
    {
        switch (e.State)
        {
            case RasConnectionState.Connecting:
                RaiseState(VpnStatus.Connecting, VpnError.None);
                break;
            case RasConnectionState.Connected:
                RaiseState(VpnStatus.Connected, VpnError.None, _rasConnection.GetLocalIpAddress() ?? string.Empty);
                break;
            case RasConnectionState.Disconnected:
                RaiseState(VpnStatus.Disconnected, e.ErrorCode == 0 ? VpnError.None : VpnError.Unknown);
                break;
        }
    }

    private void RaiseState(VpnStatus status, VpnError error, string localIp = "")
    {
        StateChanged?.Invoke(this, new EventArgs<VpnState>(new VpnState(
            status, error, localIp, _endpoint.Server.Ip, _endpoint.Port, _vpnProtocol)));
    }
}
