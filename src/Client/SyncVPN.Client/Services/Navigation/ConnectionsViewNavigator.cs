/*
 * Copyright (c) 2024 Proton AG
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
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Core.Services.Mapping;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Core.Services.Navigation.Bases;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Recents.Contracts;
using SyncVPN.Client.Logic.Recents.Contracts.Messages;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Services.FreeServers;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Countries;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Gateways;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Profiles;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Recents;
using SyncVPN.Logging.Contracts;

namespace SyncVPN.Client.Services.Navigation;

public class ConnectionsViewNavigator : ViewNavigatorBase, IConnectionsViewNavigator,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<RecentConnectionsChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>
{
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly IServersLoader _serversLoader;
    private readonly IFreeServersCache _freeServersCache;
    private readonly ISettings _settings;

    public override FrameLoadedBehavior LoadBehavior { get; protected set; } = FrameLoadedBehavior.NavigateToDefaultViewIfEmpty;

    public ConnectionsViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher,
        IRecentConnectionsManager recentConnectionsManager,
        IServersLoader serversLoader,
        IFreeServersCache freeServersCache,
        ISettings settings)
        : base(logger, pageViewMapper, uiThreadDispatcher)
    {
        _recentConnectionsManager = recentConnectionsManager;
        _serversLoader = serversLoader;
        _freeServersCache = freeServersCache;
        _settings = settings;
    }

    // The legacy Proton catalog is empty for an anonymous/free-tier session (it requires a login), but
    // the new SyncVPN backend's free-server catalog doesn't - so the Countries tab must stay reachable
    // when only that one is populated, not just when the legacy list has entries.
    public bool CanNavigateToCountriesView()
    {
        return _serversLoader.HasAnyCountries() || _freeServersCache.GetServers().Count > 0;
    }

    public async Task<bool> NavigateToCountriesViewAsync(CountriesConnectionType initialType = CountriesConnectionType.All)
    {
        return CanNavigateToCountriesView()
            && await NavigateToAsync<CountriesPageViewModel>(parameter: initialType);
    }

    public Task<bool> NavigateToSecureCoreViewAsync()
    {
        return NavigateToCountriesViewAsync(CountriesConnectionType.SecureCore);
    }

    public bool CanNavigateToGatewaysView()
    {
        return _serversLoader.HasAnyGateways();
    }

    public async Task<bool> NavigateToGatewaysViewAsync()
    {
        return CanNavigateToGatewaysView()
            && await NavigateToAsync<GatewaysPageViewModel>();
    }

    public bool CanNavigateToProfilesView()
    {
        return true;
    }

    public async Task<bool> NavigateToProfilesViewAsync()
    {
        return CanNavigateToProfilesView()
            && await NavigateToAsync<ProfilesPageViewModel>();
    }

    public bool CanNavigateToRecentsView()
    {
        return _settings.VpnPlan.IsPaid
            || _recentConnectionsManager.HasAnyRecentConnections();
    }

    public async Task<bool> NavigateToRecentsViewAsync()
    {
        return CanNavigateToRecentsView()
            && await NavigateToAsync<RecentsPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return _recentConnectionsManager.HasAnyRecentConnections()
                ? NavigateToRecentsViewAsync()
                : _serversLoader.HasAnyGateways()
                    ? NavigateToGatewaysViewAsync()
                    : NavigateToCountriesViewAsync();
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        UIThreadDispatcher.TryEnqueue(async () => await InvalidateCurrentPageAsync());
    }

    public void Receive(ServerListChangedMessage message)
    {
        UIThreadDispatcher.TryEnqueue(async () => await InvalidateCurrentPageAsync());
    }

    public void Receive(RecentConnectionsChangedMessage message)
    {
        UIThreadDispatcher.TryEnqueue(async () => await InvalidateCurrentPageAsync());
    }

    public void Receive(LoggedInMessage message)
    {
        UIThreadDispatcher.TryEnqueue(async () => await NavigateToDefaultAsync());
    }

    private async Task InvalidateCurrentPageAsync()
    {
        if (GetCurrentPageContext() is not IConnectionPage connectionPage || !connectionPage.IsAvailable)
        {
            await NavigateToDefaultAsync();
        }
    }
}