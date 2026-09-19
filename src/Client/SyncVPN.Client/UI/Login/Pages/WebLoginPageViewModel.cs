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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SyncVPN.Client.Contracts.Services.Browsing;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Core.Messages;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Enums;
using SyncVPN.Client.Logic.Auth.Contracts.Models;
using SyncVPN.Client.UI.Login.Bases;

namespace SyncVPN.Client.UI.Login.Pages;

// The default sign-in entry point (replaces SignInPageViewModel - see LoginViewNavigator.NavigateToDefaultAsync):
// OpenBrowserSignInCommand calls POST /auth/web-app (IUserAuthenticator.StartWebLoginAsync) to get a
// verification_url, opens exactly that URL in the user's browser (never reconstructed client-side), and
// then waits for it to be completed via IUserAuthenticator.WaitForWebLoginAsync polling POST
// /auth/web-app/status at the server-provided interval. The actual "signing in..." UI while that wait is
// in progress is LoadingPageViewModel - AuthenticationStatus flipping to LoggingIn (set synchronously
// inside StartWebLoginAsync) makes LoginViewNavigator hand off to it almost immediately, the same way
// every other login method already works.
//
// Deliberately NOT auto-started from OnActivated: a failed attempt sets AuthenticationStatus back to
// LoggedOut, which sends the user right back to this same page - auto-starting on activation turned that
// into an unthrottled loop that reopened the browser and re-hit the backend as fast as each attempt
// failed, taking the whole app down. Requiring an explicit click breaks that loop; a failed attempt just
// leaves the button available to try again.
public partial class WebLoginPageViewModel : LoginPageViewModelBase
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IEventMessageSender _eventMessageSender;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenBrowserSignInCommand))]
    private bool _isSigningIn;

    public WebLoginPageViewModel(
        ILoginViewNavigator parentViewNavigator,
        IUrlsBrowser urlsBrowser,
        IUserAuthenticator userAuthenticator,
        IEventMessageSender eventMessageSender,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _userAuthenticator = userAuthenticator;
        _eventMessageSender = eventMessageSender;
    }

    [RelayCommand(CanExecute = nameof(CanOpenBrowserSignIn))]
    public async Task OpenBrowserSignInAsync()
    {
        try
        {
            IsSigningIn = true;

            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Authenticating));

            WebLoginStartResult startResult = await _userAuthenticator.StartWebLoginAsync();
            if (!startResult.Success)
            {
                if (startResult.Value != AuthError.None)
                {
                    _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Error, startResult.Value, startResult.Error));
                }
                return;
            }

            _urlsBrowser.BrowseTo(startResult.VerificationUrl);

            AuthResult result = await _userAuthenticator.WaitForWebLoginAsync(startResult);

            if (result.Success)
            {
                _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Success));
            }
            else if (result.Value != AuthError.None)
            {
                // AuthError.None means the wait was cancelled (e.g. the Loading page's Cancel button) -
                // that already puts the user back here via AuthenticationStatusChanged(LoggedOut), with
                // nothing to show; anything else is a real failure worth surfacing.
                _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Error, result.Value, result.Error));
            }
        }
        finally
        {
            IsSigningIn = false;
        }
    }

    private bool CanOpenBrowserSignIn()
    {
        return !IsSigningIn;
    }

    // Navigates to CodeLoginPageView so the 16-character login_code flow (e.g. handed out after a guest
    // purchase) stays reachable now that SignInPageViewModel is no longer the default entry point.
    [RelayCommand]
    public Task NavigateToCodeLoginAsync()
    {
        return ParentViewNavigator.NavigateToCodeLoginViewAsync();
    }
}
