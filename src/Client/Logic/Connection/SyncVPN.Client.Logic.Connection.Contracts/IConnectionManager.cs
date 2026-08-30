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

using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;
using ConnectionDetails = SyncVPN.Client.Logic.Connection.Contracts.Models.ConnectionDetails;

namespace SyncVPN.Client.Logic.Connection.Contracts;

public interface IConnectionManager
{
    ConnectionStatus ConnectionStatus { get; }
    ConnectionDetails? CurrentConnectionDetails { get; }
    IConnectionIntent? CurrentConnectionIntent { get; }

    bool IsDisconnected { get; }
    bool IsConnecting { get; }
    bool IsConnected { get; }
    bool HasError { get; }
    bool IsNetworkBlocked { get; }
    bool IsTwoFactorError { get; }
    bool IsMobileHotspotError { get; }

    Task ConnectAsync(VpnTriggerDimension vpnConnectionTrigger, IConnectionIntent? connectionIntent = null);
    Task<bool> ReconnectIfNotRecentlyReconnectedAsync();
    Task<bool> ReconnectAsync(VpnTriggerDimension vpnConnectionTrigger);
    Task DisconnectAsync(VpnTriggerDimension vpnTriggerDimension);

    Task InitializeAsync(IConnectionIntent? connectionIntent);
}