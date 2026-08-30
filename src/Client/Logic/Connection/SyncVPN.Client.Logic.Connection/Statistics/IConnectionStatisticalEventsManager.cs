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

using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Logic.Connection.Statistics;

public interface IConnectionStatisticalEventsManager
{
    void SetConnectionAttempt(VpnTriggerDimension trigger, ConnectionStatus connectionStatus); 
    
    void SetReconnectionAttempt(VpnTriggerDimension trigger, ConnectionStatus currentConnectionStatus);

    void SetDisconnectionAttempt(VpnTriggerDimension trigger, ConnectionStatus connectionStatus);

    void OnVpnStateChanged(VpnStatusIpcEntity vpnStatus, VpnErrorTypeIpcEntity vpnError, ConnectionDetails? connectionDetails);
}