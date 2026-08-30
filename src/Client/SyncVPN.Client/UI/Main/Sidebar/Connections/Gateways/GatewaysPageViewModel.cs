/*
 * Copyright (c) 2023 Proton AG
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
using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.Common.UI.Assets.Icons.Base;
using SyncVPN.Client.Common.UI.Assets.Icons.PathIcons;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Models.Connections;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;

namespace SyncVPN.Client.UI.Main.Sidebar.Connections.Gateways;

public partial class GatewaysPageViewModel : ConnectionPageViewModelBase,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ILocationItemFactory _locationItemFactory;

    public override string Header => Localizer.Get("Gateways_Page_Title");

    public override IconElement Icon => new Buildings() { Size = PathIconSize.Pixels16 };

    public override int SortIndex { get; } = 4;

    public override bool IsAvailable => ParentViewNavigator.CanNavigateToGatewaysView();

    public string BannerDescription => Localizer.Get("Gateways_Page_Description");

    public bool IsInfoBannerVisible => !Settings.IsGatewayInfoBannerDismissed;

    public GatewaysPageViewModel(
        IConnectionsViewNavigator parentViewNavigator,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IConnectionGroupFactory connectionGroupFactory,
        ILocationItemFactory locationItemFactory,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator,
               settings,
               serversLoader,
               connectionManager,
               connectionGroupFactory,
               viewModelHelper)
    {
        _locationItemFactory = locationItemFactory;
    }

    protected override IEnumerable<ConnectionItemBase> GetItems()
    {
        IEnumerable<ConnectionItemBase> gateways = 
            ServersLoader.GetGateways()
                         .Select(_locationItemFactory.GetGateway);

        return gateways;
    }

    [RelayCommand]
    private void DismissInfoBanner()
    {
        Settings.IsGatewayInfoBannerDismissed = true;
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsGatewayInfoBannerDismissed))
        {
            ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsInfoBannerVisible)));
        }
    }

    protected override void OnServerListChanged()
    {
        base.OnServerListChanged();

        OnPropertyChanged(nameof(IsAvailable));
    }
}