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

// Exchanges the 16-character login_code the SyncVPN backend hands out (e.g. after a guest purchase - see
// IUserAuthenticator.LoginWithCodeAsync's doc comment) for a device session via POST /auth/code-login.
// No legacy Proton equivalent - always goes through SyncVpnAuthenticator regardless of BackendCapability.Auth.
public partial class CodeLoginPageViewModel : LoginPageViewModelBase
{
    private const int CODE_LENGTH = 16;

    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUserAuthenticator _userAuthenticator;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private bool _isToShowError;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SignInWithCodeCommand))]
    [NotifyPropertyChangedFor(nameof(IsFormEnabled))]
    private bool _isSigningIn;

    public bool IsFormEnabled => !IsSigningIn;

    public CodeLoginPageViewModel(
        ILoginViewNavigator parentViewNavigator,
        IEventMessageSender eventMessageSender,
        IUserAuthenticator userAuthenticator,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, viewModelHelper)
    {
        _eventMessageSender = eventMessageSender;
        _userAuthenticator = userAuthenticator;
    }

    [RelayCommand(CanExecute = nameof(CanSignIn))]
    public async Task SignInWithCodeAsync()
    {
        string trimmedCode = Code.Trim();
        if (trimmedCode.Length != CODE_LENGTH)
        {
            IsToShowError = true;
            return;
        }

        IsToShowError = false;

        try
        {
            IsSigningIn = true;

            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Authenticating));

            AuthResult result = await _userAuthenticator.LoginWithCodeAsync(trimmedCode);

            if (result.Success)
            {
                Code = string.Empty;
                _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Success));
            }
            else if (result.Value == AuthError.TwoFactorRequired)
            {
                // The account this code belongs to has 2FA enabled - same challenge/verify flow as a
                // password login (LoginPageViewModel.Receive routes this to TwoFactorPageView regardless
                // of which page raised it).
                _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.TwoFactorRequired));
            }
            else
            {
                // Routed through the generic LoginState.Error path (LoginPageViewModel navigates back to
                // Sign-In and shows the message there) rather than a local error here, so this page
                // doesn't need its own copy of every AuthError's message mapping.
                _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Error, result.Value, result.Error));
            }
        }
        catch (Exception ex)
        {
            _eventMessageSender.Send(new LoginStateChangedMessage(LoginState.Error, AuthError.Unknown, ex.Message));
        }
        finally
        {
            IsSigningIn = false;
        }
    }

    private bool CanSignIn()
    {
        return !IsSigningIn;
    }

    [RelayCommand]
    private Task<bool> GoBackAsync()
    {
        return ParentViewNavigator.NavigateToWebLoginViewAsync();
    }

    partial void OnCodeChanged(string value)
    {
        IsToShowError = false;
    }

    protected override void OnDeactivated()
    {
        IsToShowError = false;
    }
}
