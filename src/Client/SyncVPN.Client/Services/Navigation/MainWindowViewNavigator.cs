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
using SyncVPN.Client.Core.Services.Mapping;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Core.Services.Navigation.Bases;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Servers.Cache;
using SyncVPN.Client.Logic.Users.Contracts;
using SyncVPN.Client.UI.Login;
using SyncVPN.Client.UI.Main;
using SyncVPN.Logging.Contracts;

namespace SyncVPN.Client.Services.Navigation;

public class MainWindowViewNavigator : ViewNavigatorBase, IMainWindowViewNavigator,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    private readonly IServersCache _serversCache;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IVpnPlanUpdater _vpnPlanUpdater;

    public MainWindowViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher,
        IServersCache serversCache,
        IUserAuthenticator userAuthenticator,
        IVpnPlanUpdater vpnPlanUpdater)
        : base(logger, pageViewMapper, uiThreadDispatcher)
    {
        _serversCache = serversCache;
        _userAuthenticator = userAuthenticator;
        _vpnPlanUpdater = vpnPlanUpdater;
    }

    public Task<bool> NavigateToLoginViewAsync()
    {
        return _vpnPlanUpdater.AuthResponseDetails is null
            ? NavigateToAsync<LoginPageViewModel>()
            : NavigateToNoServersViewAsync();
    }

    public Task<bool> NavigateToMainViewAsync()
    {
        return NavigateToAsync<MainPageViewModel>();
    }

    public Task<bool> NavigateToNoServersViewAsync()
    {
        return NavigateToAsync<NoServersPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return _userAuthenticator.IsLoggedIn
            ? _serversCache.HasNoServers()
                ? NavigateToNoServersViewAsync()
                : NavigateToMainViewAsync()
            : NavigateToLoginViewAsync();
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        UIThreadDispatcher.TryEnqueue(async () =>
        {
            await NavigateToDefaultAsync();
        });
    }
}