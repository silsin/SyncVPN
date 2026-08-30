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

using SyncVPN.Common.Core.Extensions;
using SyncVPN.Common.Core.Networking;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.NetworkLogs;
using SyncVPN.OperatingSystems.Network.Contracts;
using SyncVPN.Vpn.Common;

namespace SyncVPN.Service.Vpn;

public class NetworkSettings : IVpnStateAware
{
    private readonly ILogger _logger;
    private readonly INetworkInterfaceLoader _networkInterfaceLoader;
    private readonly INetworkUtilities _networkUtilities;
    private readonly WintunRegistryFixer _wintunRegistryFixer;

    public NetworkSettings(
        ILogger logger,
        INetworkInterfaceLoader networkInterfaceLoader,
        INetworkUtilities networkUtilities,
        WintunRegistryFixer wintunRegistryFixer)
    {
        _logger = logger;
        _networkInterfaceLoader = networkInterfaceLoader;
        _networkUtilities = networkUtilities;
        _wintunRegistryFixer = wintunRegistryFixer;
    }

    public void OnVpnDisconnected(VpnState state)
    {
        if (state.VpnProtocol.IsOpenVpn())
        {
            RestoreNetworkSettings(state.VpnProtocol, state.OpenVpnAdapter);
        }
    }

    public void OnVpnConnected(VpnState state)
    {
    }

    public void AssigningIp(VpnState state)
    {
    }

    public void OnVpnConnecting(VpnState state)
    {
        if (state.VpnProtocol == VpnProtocol.OpenVpnTcp || state.VpnProtocol == VpnProtocol.OpenVpnUdp)
        {
            ApplyNetworkSettings(state.VpnProtocol, state.OpenVpnAdapter);
            _wintunRegistryFixer.EnsureTunAdapterRegistryIsCorrect();
        }
    }

    private void ApplyNetworkSettings(VpnProtocol vpnProtocol, OpenVpnAdapter? openVpnAdapter)
    {
        uint interfaceIndex = _networkInterfaceLoader.GetByVpnProtocol(vpnProtocol, openVpnAdapter).Index;

        try
        {
            _logger.Info<NetworkLog>("Setting interface metric...");
            _networkUtilities.SetLowestTapMetric(interfaceIndex);
            _logger.Info<NetworkLog>("Interface metric set.");
        }
        catch (NetworkUtilException e)
        {
            _logger.Error<NetworkLog>("Failed to apply network settings. Error code: " + e.Code);
        }
    }

    private void RestoreNetworkSettings(VpnProtocol vpnProtocol, OpenVpnAdapter? openVpnAdapter)
    {
        uint interfaceIndex = _networkInterfaceLoader.GetByVpnProtocol(vpnProtocol, openVpnAdapter)?.Index ?? 0;
        if (interfaceIndex == 0)
        {
            return;
        }

        try
        {
            _logger.Info<NetworkLog>("Restoring interface metric...");
            _networkUtilities.RestoreDefaultTapMetric(interfaceIndex);
            _logger.Info<NetworkLog>("Interface metric restored.");
        }
        catch (NetworkUtilException e)
        {
            _logger.Error<NetworkLog>("Failed restore network settings. Error code: " + e.Code);
        }
    }
}