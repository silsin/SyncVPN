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

using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Handlers.Bases;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Servers.Cache;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Enums;
using SyncVPN.Client.Settings.Contracts.Models;
using SyncVPN.Client.UI.Main;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.ConnectionLogs;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Handlers;

public class NoServersHandler : IHandler,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<NoVpnConnectionsAssignedMessage>
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IServersCache _serversCache;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IMainWindowViewNavigator _mainWindowViewNavigator;
    private readonly IConnectionManager _connectionManager;

    public NoServersHandler(
        ILogger logger,
        ISettings settings,
        IServersCache serversCache,
        IUIThreadDispatcher uiThreadDispatcher,
        IUserAuthenticator userAuthenticator,
        IMainWindowViewNavigator mainWindowViewNavigator,
        IConnectionManager connectionManager)
    {
        _logger = logger;
        _settings = settings;
        _serversCache = serversCache;
        _uiThreadDispatcher = uiThreadDispatcher;
        _userAuthenticator = userAuthenticator;
        _mainWindowViewNavigator = mainWindowViewNavigator;
        _connectionManager = connectionManager;
    }

    public async void Receive(ServerListChangedMessage message)
    {
        if (!_userAuthenticator.IsLoggedIn)
        {
            return;
        }

        await _uiThreadDispatcher.TryEnqueueAsync(async () =>
        {
            if (_serversCache.HasNoServers())
            {
                await HandleNoServersAsync();
            }
            else if (_mainWindowViewNavigator.GetCurrentPageContext() is NoServersPageViewModel)
            {
                await _mainWindowViewNavigator.NavigateToMainViewAsync();
            }

            HandleDefaultConnectionSetting();
        });
    }

    private void HandleDefaultConnectionSetting()
    {
        if (_serversCache.Gateways.Any() && !_serversCache.Countries.Any() &&
            _settings.DefaultConnection.Type is DefaultConnectionType.Fastest or DefaultConnectionType.Random)
        {
            _settings.DefaultConnection = DefaultConnection.Last;
        }
    }

    private async Task HandleNoServersAsync()
    {
        if (!_connectionManager.IsDisconnected)
        {
            _logger.Info<ConnectionLog>("Disconnecting from VPN due to no servers available.");

            await _connectionManager.DisconnectAsync(VpnTriggerDimension.Auto);
        }

        await _mainWindowViewNavigator.NavigateToNoServersViewAsync();
    }

    public void Receive(LoggedInMessage message)
    {
        _uiThreadDispatcher.TryEnqueue(HandleDefaultConnectionSetting);
    }

    public async void Receive(NoVpnConnectionsAssignedMessage message)
    {
        await _uiThreadDispatcher.TryEnqueueAsync(HandleNoServersAsync);
    }
}