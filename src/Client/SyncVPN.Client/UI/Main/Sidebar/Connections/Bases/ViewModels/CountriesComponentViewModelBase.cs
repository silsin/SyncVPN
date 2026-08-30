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
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Models.Connections;
using SyncVPN.Client.Services.Upselling;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;

public abstract partial class CountriesComponentViewModelBase : ActivatableViewModelBase, ICountriesComponent,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>
{
    protected readonly ISettings Settings;
    protected readonly IServersLoader ServersLoader;
    protected readonly ILocationItemFactory LocationItemFactory;
    private readonly IAccountUpgradeUrlLauncher _accountUpgradeUrlLauncher;

    public abstract CountriesConnectionType ConnectionType { get; }

    public abstract int SortIndex { get; }
    public abstract string Header { get; }
    public abstract string Description { get; }
    public abstract bool IsInfoBannerVisible { get; }

    public bool IsUpsellBannerVisible => IsRestricted;
    public bool IsRestricted => !Settings.VpnPlan.IsPaid;
    protected abstract ModalSource UpsellModalSource { get; }

    protected CountriesComponentViewModelBase(
        ISettings settings,
        IServersLoader serversLoader,
        ILocationItemFactory locationItemFactory,
        IViewModelHelper viewModelHelper,
        IAccountUpgradeUrlLauncher accountUpgradeUrlLauncher)
        : base(viewModelHelper)
    {
        Settings = settings;
        ServersLoader = serversLoader;
        LocationItemFactory = locationItemFactory;
        _accountUpgradeUrlLauncher = accountUpgradeUrlLauncher;
    }

    public virtual IEnumerable<ConnectionItemBase> GetItems()
    {
        IEnumerable<ConnectionItemBase> genericCountries =
        [
            LocationItemFactory.GetGenericCountry(ConnectionType, SelectionStrategy.Fastest, false),

            // Do not include 'Fastest (excluding my country)' and 'Random country' in the options
            //LocationItemFactory.GetGenericCountry(ConnectionType, SelectionStrategy.Fastest, true),
            //LocationItemFactory.GetGenericCountry(ConnectionType, SelectionStrategy.Random, false),
        ];

        return genericCountries;
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() => OnSettingsChanged(message.PropertyName));
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    [RelayCommand]
    protected abstract void DismissInfoBanner();

    protected virtual void OnSettingsChanged(string propertyName)
    { }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(Description));
    }

    [RelayCommand]
    private async Task UpgradeAsync()
    {
        await _accountUpgradeUrlLauncher.OpenAsync(UpsellModalSource);
    }
}