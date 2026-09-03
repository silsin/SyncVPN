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

using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Core.Services.Mapping;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Core.Services.Navigation.Bases;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.UI.Main.Settings;
using SyncVPN.Client.UI.Main.Sidebar.Connections;
using SyncVPN.Logging.Contracts;

namespace SyncVPN.Client.Services.Navigation;

public class MainViewNavigator : ViewNavigatorBase, IMainViewNavigator,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<LoggedOutMessage>
{
    private readonly IConnectionsViewNavigator _connectionsViewNavigator;

    private ConnectionStatus _connectionStatus = ConnectionStatus.Disconnected;

    public MainViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher,
        IConnectionsViewNavigator connectionsViewNavigator)
        : base(logger, pageViewMapper, uiThreadDispatcher)
    {
        _connectionsViewNavigator = connectionsViewNavigator;
    }

    public Task<bool> NavigateToHomeViewAsync(bool forceNavigation = false)
    {
        return ClearFrameAsync(forceNavigation);
    }

    public Task<bool> NavigateToSettingsViewAsync()
    {
        return NavigateToAsync<SettingsPageViewModel>();
    }

    public async Task<bool> NavigateToCountriesViewAsync()
    {
        bool navigated = await NavigateToAsync<ConnectionsPageViewModel>();
        if (navigated)
        {
            await _connectionsViewNavigator.NavigateToCountriesViewAsync();
        }

        return navigated;
    }

    public async Task<bool> NavigateToProfilesViewAsync()
    {
        bool navigated = await NavigateToAsync<ConnectionsPageViewModel>();
        if (navigated)
        {
            await _connectionsViewNavigator.NavigateToProfilesViewAsync();
        }

        return navigated;
    }

    public async Task<bool> NavigateToSecureCoreViewAsync()
    {
        bool navigated = await NavigateToAsync<ConnectionsPageViewModel>();
        if (navigated)
        {
            await _connectionsViewNavigator.NavigateToSecureCoreViewAsync();
        }

        return navigated;
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return NavigateToHomeViewAsync();
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (_connectionStatus == message.ConnectionStatus)
        {
            return;
        }

        _connectionStatus = message.ConnectionStatus;

        if (message.ConnectionStatus == ConnectionStatus.Connecting)
        {
            // Force navigation to automatically discard any unsaved changes
            UIThreadDispatcher.TryEnqueue(async () => await NavigateToHomeViewAsync(forceNavigation: true));
        }
    }

    public void Receive(LoggedOutMessage message)
    {
        // Force navigation to automatically discard any unsaved changes
        UIThreadDispatcher.TryEnqueue(async () => await NavigateToHomeViewAsync(forceNavigation: true));
    }
}