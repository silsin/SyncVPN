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

using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Logic.Connection.ConnectionErrors;

// Was falling through to UnknownConnectionError's generic "something went wrong" message - the service
// actually knows exactly what happened here (VpnEndpointScanner tried every port for the server and none
// answered within ConnectionManager.ConnectingWatchdogTimeout), so this gives the user that specific,
// actionable reason instead.
public class PingTimeoutConnectionError : ConnectionErrorBase
{
    private readonly IConnectionManager _connectionManager;

    public override string Message => Localizer.Get("Connection_Error_PingTimeout");

    public override string ActionLabel => Localizer.Get("Common_Actions_TryAgain");

    public PingTimeoutConnectionError(ILocalizationProvider localizer, IConnectionManager connectionManager)
        : base(localizer)
    {
        _connectionManager = connectionManager;
    }

    public override Task ExecuteActionAsync()
    {
        return _connectionManager.ReconnectAsync(VpnTriggerDimension.NewConnection);
    }
}
