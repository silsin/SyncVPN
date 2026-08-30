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

using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.Common.UI.Assets.Icons.Base;
using SyncVPN.Client.Common.UI.Assets.Icons.PathIcons;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Recents.Contracts;
using SyncVPN.Client.Logic.Recents.Contracts.Messages;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Models.Connections;
using SyncVPN.Client.Models.Connections.Recents;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;
using SyncVPN.Common.Core.Geographical;

namespace SyncVPN.Client.UI.Main.Sidebar.Connections.Recents;

public class RecentsPageViewModel : ConnectionPageViewModelBase,
    IEventMessageReceiver<RecentConnectionsChangedMessage>
{
    private readonly IConnectionItemFactory _connectionItemFactory;
    private readonly IRecentConnectionsManager _recentConnectionsManager;

    public override string Header => Localizer.Get("Home_Recents_Title");

    public override IconElement Icon => new ClockRotateLeft() { Size = PathIconSize.Pixels16 };

    public override int SortIndex { get; } = 1;

    public override bool IsAvailable => ParentViewNavigator.CanNavigateToRecentsView();

    public RecentsPageViewModel(
        IConnectionsViewNavigator parentViewNavigator,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IConnectionGroupFactory connectionGroupFactory,
        IConnectionItemFactory connectionItemFactory, 
        IRecentConnectionsManager recentConnectionsManager,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator,
               settings,
               serversLoader,
               connectionManager,
               connectionGroupFactory,
               viewModelHelper)
    {
        _connectionItemFactory = connectionItemFactory;
        _recentConnectionsManager = recentConnectionsManager;
    }

    public void Receive(RecentConnectionsChangedMessage message)
    {
        ExecuteOnUIThread(FetchItems);
    }

    protected override IEnumerable<ConnectionItemBase> GetItems()
    {
        return _recentConnectionsManager.GetRecentConnections()
                                        .Select(_connectionItemFactory.GetRecent);
    }

    protected override void InvalidateMaintenanceStates()
    {
        if (IsActive)
        {
            IEnumerable<Server> servers = ServersLoader.GetServers();
            DeviceLocation? deviceLocation = Settings.DeviceLocation;

            foreach (RecentConnectionItem item in Items.OfType<RecentConnectionItem>())
            {
                item.InvalidateIsUnderMaintenance(servers, deviceLocation);
            }
        }
    }

    protected override void OnVpnPlanChanged(VpnPlan oldPlan, VpnPlan newPlan)
    {
        base.OnVpnPlanChanged(oldPlan, newPlan);

        InvalidateIsAvailable();
    }

    protected override void OnLoggedIn()
    {
        base.OnLoggedIn();

        InvalidateIsAvailable();
    }

    protected override void OnItemsChanged()
    {
        base.OnItemsChanged();

        InvalidateIsAvailable();
    }

    private void InvalidateIsAvailable()
    {
        OnPropertyChanged(nameof(IsAvailable));
    }
}