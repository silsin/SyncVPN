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

using SyncVPN.Api.Contracts;
using SyncVPN.Api.Contracts.Servers;
using SyncVPN.Client.Common.Observers;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.IssueReporting.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;
using SyncVPN.Logging.Contracts.Events.ConnectLogs;

namespace SyncVPN.Client.Logic.Connection;

public class ConnectedServerChecker : PollingObserverBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;
    private readonly IServersLoader _serversLoader;
    private readonly IApiClient _apiClient;
    private readonly IServersUpdater _serversUpdater;

    protected override TimeSpan PollingInterval => _settings.ConnectedServerCheckInterval.AddJitter(0.2);

    public ConnectedServerChecker(ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IConnectionManager connectionManager,
        IServersLoader serversLoader,
        IApiClient apiClient,
        IServersUpdater serversUpdater) 
        : base(logger, issueReporter)
    {
        _logger = logger;
        _settings = settings;
        _connectionManager = connectionManager;
        _serversLoader = serversLoader;
        _apiClient = apiClient;
        _serversUpdater = serversUpdater;
    }

    protected override async Task OnTriggerAsync()
    {
        await CheckIfCurrentServerIsOnlineAsync();
    }

    public async Task CheckIfCurrentServerIsOnlineAsync()
    {
        if (IsTimerEnabled && await IsToReconnectAsync())
        {
            Logger.Info<ConnectTriggerLog>($"Refreshing the server list due to the current connected server being no longer available.");
            await _serversUpdater.ForceUpdateAsync();
        }
    }

    private async Task<bool> IsToReconnectAsync()
    {
        ConnectionDetails? connectionDetails = _connectionManager.CurrentConnectionDetails;
        if (connectionDetails is null)
        {
            Logger.Info<AppLog>("There are no connection details for the connected server.");
            return false;
        }
        if (connectionDetails.ServerId is null)
        {
            Logger.Info<AppLog>("There is no Server ID in the connection details of the connected server.");
            return false;
        }
        if (connectionDetails.PhysicalServerId is null)
        {
            Logger.Info<AppLog>("There is no Physical Server ID in the connection details of the connected server.");
            return false;
        }

        string serverId = connectionDetails.ServerId;
        string physicalServerId = connectionDetails.PhysicalServerId;
        try
        {
            Server? server = _serversLoader.GetById(serverId);
            if (server is null)
            {
                Logger.Info<AppLog>($"The connected server doesn't exist in the server list. " +
                    $"Reconnecting. (Server ID '{serverId}')");
                return true;
            }

            PhysicalServer? physicalServer = server.Servers.FirstOrDefault(ps => ps.Id == physicalServerId);
            if (physicalServer is null)
            {
                Logger.Info<AppLog>($"The connected physical server doesn't exist in the server list. " +
                    $"Reconnecting. (Physical Server ID '{physicalServerId}', Server ID '{serverId}')");
                return true;
            }

            ApiResponseResult<PhysicalServerWrapperResponse> result = await _apiClient.GetServerAsync(physicalServerId);
            if (result.Failure)
            {
                Logger.Error<AppLog>($"Failed to check the connected server through the API. " +
                    $"(Physical Server ID '{physicalServerId}').");
                return false;
            }

            bool isServerUnderMaintenance = result.Value.Server.Status == 0;
            if (isServerUnderMaintenance)
            {
                Logger.Info<AppLog>($"The connected server is under maintenance. " +
                    $"Reconnecting. (Physical Server ID '{physicalServerId}')");
                MarkServerAsUnderMaintenance(server, result.Value.Server);
            }

            _logger.Info<AppLog>($"Current server {server.Name} load: {server.Load}, score: {server.Score}.");

            return isServerUnderMaintenance;
        }
        catch (Exception ex)
        {
            Logger.Info<AppLog>($"An unexpected exception occurred when checking the connected server. " +
                $"(Server ID '{serverId}' Physical Server ID '{physicalServerId}')", ex);
            return false;
        }
    }

    private void MarkServerAsUnderMaintenance(Server server, PhysicalServerResponse newPhysicalServer)
    {
        PhysicalServer? physicalServer = server.Servers.FirstOrDefault(ps => ps.Id == newPhysicalServer.Id);
        if (physicalServer is null)
        {
            return;
        }

        physicalServer.Status = newPhysicalServer.Status;

        // Set logical server to under maintenance if all physical servers are under maintenance
        if (server.Servers.All(ps => ps.IsUnderMaintenance()))
        {
            server.Status = 0;
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        SetTimer();
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsFeatureConnectedServerCheckEnabled))
        {
            SetTimer();
        }
        else if (message.PropertyName == nameof(ISettings.ConnectedServerCheckInterval))
        {
            // If the interval changes, we need to stop the timer, change the interval and start the timer again
            StopTimer();
            SetTimer();
        }
    }

    private void SetTimer()
    {
        if (_connectionManager.IsConnected && _settings.IsFeatureConnectedServerCheckEnabled)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }
}