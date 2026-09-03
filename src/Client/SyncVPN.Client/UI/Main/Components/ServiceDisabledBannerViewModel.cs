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
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Services.Contracts;
using SyncVPN.Client.Logic.Services.Contracts.Messages;

namespace SyncVPN.Client.UI.Main.Components;

public partial class ServiceDisabledBannerViewModel : ViewModelBase,
    IEventMessageReceiver<ServiceEnablementChangedMessage>
{
    private readonly IServiceManager _serviceManager;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EnableServiceCommand))]
    private bool _isServiceDisabledBannerVisible;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EnableServiceCommand))]
    private bool _isEnabling;

    public ServiceDisabledBannerViewModel(
        IServiceManager serviceManager,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _serviceManager = serviceManager;
        IsServiceDisabledBannerVisible = !_serviceManager.IsServiceEnabled;
    }

    public void Receive(ServiceEnablementChangedMessage message)
    {
        ExecuteOnUIThread(() => IsServiceDisabledBannerVisible = !message.IsEnabled);
    }

    [RelayCommand(CanExecute = nameof(CanEnableService))]
    private async Task EnableServiceAsync()
    {
        IsEnabling = true;

        try
        {
            await _serviceManager.EnableServiceAsync();
        }
        finally
        {
            IsEnabling = false;
        }
    }

    private bool CanEnableService()
    {
        return IsServiceDisabledBannerVisible && !IsEnabling;
    }
}
