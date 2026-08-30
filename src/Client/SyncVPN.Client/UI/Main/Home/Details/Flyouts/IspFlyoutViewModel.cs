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

using CommunityToolkit.Mvvm.ComponentModel;
using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Contracts.Services.Browsing;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;

namespace SyncVPN.Client.UI.Main.Home.Details.Flyouts;

public partial class IspFlyoutViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<DeviceLocationChangedMessage>
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly ISettings _settings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDeviceExposed))]
    private string _isp = EmptyValueExtensions.DEFAULT;

    public string IspLearnMoreUri => _urlsBrowser.IspLearnMore;
    public bool IsDeviceExposed => !string.IsNullOrWhiteSpace(_settings.DeviceLocation?.Isp);

    public IspFlyoutViewModel(
        IUrlsBrowser urlsBrowser,
        ISettings settings,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _settings = settings;
    }

    public void Receive(DeviceLocationChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateIsp);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateIsp();
    }

    private void InvalidateIsp()
    {
        Isp = EmptyValueExtensions.GetValueOrDefault(_settings.DeviceLocation?.Isp);
    }
}