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
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;

namespace SyncVPN.Client.UI.Main.Home.Details.Flyouts;

public partial class CountryFlyoutViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<DeviceLocationChangedMessage>
{
    private readonly ISettings _settings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountryName))]
    [NotifyPropertyChangedFor(nameof(IsDeviceExposed))]
    private string _countryCode = string.Empty;

    public string CountryName => EmptyValueExtensions.TransformValueOrDefault(CountryCode, cc => Localizer.GetCountryName(cc));

    public bool IsDeviceExposed => !string.IsNullOrWhiteSpace(CountryCode);

    public CountryFlyoutViewModel(
        ISettings settings,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _settings = settings;
    }

    public void Receive(DeviceLocationChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateCountry);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateCountry();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(CountryName));
    }

    private void InvalidateCountry()
    {
        CountryCode = _settings.DeviceLocation?.CountryCode ?? string.Empty;
    }
}