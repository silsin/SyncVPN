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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Enums;
using SyncVPN.Client.UI.Login.Bases;

namespace SyncVPN.Client.UI.Login.Pages;

public partial class LoadingPageViewModel : LoginPageViewModelBase
{
    private readonly TimeSpan _longLoginThreshold = TimeSpan.FromSeconds(15);

    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUserAuthenticator _userAuthenticator;

    private IDispatcherTimer? _timer;

    public string? Message => _userAuthenticator.AuthenticationStatus switch
    {
        AuthenticationStatus.LoggingIn => Localizer.Get(_userAuthenticator.IsAutoLogin == true
            ? "Main_Loading"
            : "Main_Loading_SigningIn"),
        AuthenticationStatus.LoggingOut => Localizer.Get("Main_Loading_SigningOut"),
        _ => null
    };

    public bool IsSigningIn => _userAuthenticator.AuthenticationStatus == AuthenticationStatus.LoggingIn &&
                               _userAuthenticator.IsAutoLogin != true;

    [ObservableProperty]
    private bool _isSignInTakingLongerThanExpected;

    public LoadingPageViewModel(
        IUIThreadDispatcher uiThreadDispatcher,
        IEventMessageSender eventMessageSender,
        IUserAuthenticator userAuthenticator,
        ILoginViewNavigator parentViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, viewModelHelper)
    {
        _uiThreadDispatcher = uiThreadDispatcher;
        _eventMessageSender = eventMessageSender;
        _userAuthenticator = userAuthenticator;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        if (IsSigningIn)
        {
            _timer = _uiThreadDispatcher.GetTimer(_longLoginThreshold);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        StopTimer();
        IsSignInTakingLongerThanExpected = false;
    }

    private void OnTimerTick(object? sender, object e)
    {
        StopTimer();
        IsSignInTakingLongerThanExpected = true;
    }

    [RelayCommand]
    private void CancelSignIn()
    {
        _userAuthenticator.CancelAuth();
    }

    private void StopTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }
    }
}