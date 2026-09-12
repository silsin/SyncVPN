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
using SyncVPN.Client.UI.Dialogs.Tray.Pages;
using SyncVPN.Logging.Contracts;

namespace SyncVPN.Client.Services.Navigation;

public class TrayAppViewNavigator : ViewNavigatorBase, ITrayAppViewNavigator,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IBackendModeProvider _backendModeProvider;

    public TrayAppViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher,
        IUserAuthenticator userAuthenticator,
        IBackendModeProvider backendModeProvider)
        : base(logger,
               pageViewMapper,
               uiThreadDispatcher)
    {
        _userAuthenticator = userAuthenticator;
        _backendModeProvider = backendModeProvider;
    }

    // See MainWindowViewNavigator.IsGuestAccessEnabled.
    private bool IsGuestAccessEnabled => _backendModeProvider.IsNewBackendEnabled(BackendCapability.DeviceRegistration);

    public Task<bool> NavigateToLoginViewAsync()
    {
        return NavigateToAsync<TrayLoginPageViewModel>();
    }

    public Task<bool> NavigateToMainViewAsync()
    {
        return NavigateToAsync<TrayMainPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return _userAuthenticator.IsLoggedIn || IsGuestAccessEnabled
            ? NavigateToMainViewAsync()
            : NavigateToLoginViewAsync();
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        // See MainWindowViewNavigator.Receive - same premature-navigation risk from the transient
        // LoggingIn/LoggingOut statuses via the IsGuestAccessEnabled fallback below.
        if (message.AuthenticationStatus is AuthenticationStatus.LoggingIn or AuthenticationStatus.LoggingOut)
        {
            return;
        }

        // A failed login attempt from the tray's own sign-in page shouldn't get silently swallowed by
        // the guest-access fallback either - see MainWindowViewNavigator.Receive.
        if (message.AuthenticationStatus == AuthenticationStatus.LoggedOut && GetCurrentPageContext() is TrayLoginPageViewModel)
        {
            return;
        }

        UIThreadDispatcher.TryEnqueue(async () =>
        {
            await NavigateToDefaultAsync();
        });
    }
}