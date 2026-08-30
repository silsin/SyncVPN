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

using CommunityToolkit.Mvvm.Input;
using SyncVPN.Client.Contracts.Services.Browsing;
using SyncVPN.Client.Contracts.Services.Lifecycle;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Services.SignoutHandling;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Extensions;

namespace SyncVPN.Client.UI.Main.Settings.Pages;

public partial class UserDetailsComponentViewModel : PageViewModelBase,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly ISignOutHandler _signoutHandler;
    private readonly ISettings _settings;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IAppExitInvoker _appExitInvoker;

    public string Username => _settings.GetUsername();

    public string VpnPlan => Localizer.GetVpnPlanName(_settings.VpnPlan);

    public bool IsVpnPlan => _settings.VpnPlan.IsVpnPlan;

    public bool IsProtonPlan => _settings.VpnPlan.IsProtonPlan;

    public UserDetailsComponentViewModel(
        IUrlsBrowser urlsBrowser,
        ISignOutHandler signoutHandler,
        ISettings settings,
        IWebAuthenticator webAuthenticator,
        IAppExitInvoker appExitInvoker,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _signoutHandler = signoutHandler;
        _settings = settings;
        _webAuthenticator = webAuthenticator;
        _appExitInvoker = appExitInvoker;
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(IsVpnPlan));
            OnPropertyChanged(nameof(IsProtonPlan));
            OnPropertyChanged(nameof(VpnPlan));
        });
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(VpnPlan));
    }

    [RelayCommand]
    private async Task OpenMyAccountUrlAsync()
    {
        _urlsBrowser.BrowseTo(await _webAuthenticator.GetMyAccountUrlAsync());
    }

    [RelayCommand]
    private async Task ExitApplicationAsync()
    {
        await _appExitInvoker.ExitWithConfirmationAsync();
    }

    [RelayCommand]
    private Task SignOutAsync()
    {
        return _signoutHandler.SignOutAsync();
    }
}