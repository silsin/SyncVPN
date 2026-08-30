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

using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Handlers.Bases;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Handlers;

public class ServerMaintenanceHandler : IHandler, IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly ILogger _logger;
    private readonly IConnectionManager _connectionManager;
    private readonly IServersLoader _serversLoader;

    public ServerMaintenanceHandler(
        ILogger logger,
        IConnectionManager connectionManager,
        IServersLoader serversLoader)
    {
        _logger = logger;
        _connectionManager = connectionManager;
        _serversLoader = serversLoader;
    }

    public async void Receive(ServerListChangedMessage message)
    {
        if (_connectionManager.IsConnected && _connectionManager.CurrentConnectionDetails is not null)
        {
            Server? server = _serversLoader.GetById(_connectionManager.CurrentConnectionDetails.ServerId);
            if (server is null || server.IsUnderMaintenance())
            {
                string reason = server is null ? "removed from the API" : "put under maintenance";
                _logger.Info<AppLog>($"The current server {_connectionManager.CurrentConnectionDetails.ServerName} " +
                                     $"was {reason}, connecting to the default intent.");
                await _connectionManager.ReconnectAsync(VpnTriggerDimension.Auto);
            }
        }
    }
}