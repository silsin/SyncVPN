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

using SyncVPN.Api.BackendSelection;
using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Core.Services.Mapping;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Core.Services.Navigation.Bases;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Enums;
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
    private readonly IBackendModeProvider _backendModeProvider;

    public MainWindowViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher,
        IServersCache serversCache,
        IUserAuthenticator userAuthenticator,
        IVpnPlanUpdater vpnPlanUpdater,
        IBackendModeProvider backendModeProvider)
        : base(logger, pageViewMapper, uiThreadDispatcher)
    {
        _serversCache = serversCache;
        _userAuthenticator = userAuthenticator;
        _vpnPlanUpdater = vpnPlanUpdater;
        _backendModeProvider = backendModeProvider;
    }

    // A device-registered guest (no Proton account login) can reach Main directly. Free servers,
    // plans, etc. come from the new SyncVPN backend keyed off just the Deviceid header, not the legacy
    // Proton server cache, so the logged-in HasNoServers()/login requirement doesn't apply to this path.
    private bool IsGuestAccessEnabled => _backendModeProvider.IsNewBackendEnabled(BackendCapability.DeviceRegistration);

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
        // IsGuestAccessEnabled means the new SyncVPN backend's server catalog (IFreeServersCache) is
        // what's actually driving this app, not the legacy Proton _serversCache below - that stays
        // permanently empty for a SyncVpn-only account, since it's never populated from this backend.
        // Checking it regardless of login state (as the code used to do for IsLoggedIn) meant a real,
        // successfully logged-in SyncVPN user was unconditionally routed to "No VPN connections
        // available" the moment IsLoggedIn actually became true, even with a full Pro server catalog
        // sitting in IFreeServersCache - this already contradicted this class's own comment above.
        if (IsGuestAccessEnabled)
        {
            return NavigateToMainViewAsync();
        }

        if (_userAuthenticator.IsLoggedIn)
        {
            return _serversCache.HasNoServers()
                ? NavigateToNoServersViewAsync()
                : NavigateToMainViewAsync();
        }

        return NavigateToLoginViewAsync();
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        // LoggingIn/LoggingOut are transient - reacting to them here would send whoever just pressed
        // "Sign in" (or "Sign in with a code", or auto-login at startup) straight to Main through the
        // IsGuestAccessEnabled fallback below, before their login attempt has even called the API and
        // resolved. Only react once the status has actually settled.
        if (message.AuthenticationStatus is AuthenticationStatus.LoggingIn or AuthenticationStatus.LoggingOut)
        {
            return;
        }

        // A settled LoggedOut while the user is still sitting on the Login flow means a login attempt
        // they explicitly started (from Settings' "Sign in", not the guest cold-start path - that one
        // never reaches this handler at all, see AutoLoginUserAsync's no-session branch) just failed.
        // LoginPageViewModel.Receive(LoginStateChangedMessage) already put the error on screen and kept
        // them on Sign-In - don't let IsGuestAccessEnabled below yank them back to Main and silently
        // discard that error the moment they typed the wrong thing.
        if (message.AuthenticationStatus == AuthenticationStatus.LoggedOut && GetCurrentPageContext() is LoginPageViewModel)
        {
            return;
        }

        UIThreadDispatcher.TryEnqueue(async () =>
        {
            await NavigateToDefaultAsync();
        });
    }
}