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

using Microsoft.UI.Xaml.Data;
using SyncVPN.Client.Common.Collections;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Services.Navigation.Bases;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Models.Connections;
using SyncVPN.Client.Settings.Contracts;

namespace SyncVPN.Client.UI.Main.Sidebar.Bases;

public abstract class ConnectionListViewModelBase<TParentViewNavigator> : PageViewModelBase<TParentViewNavigator>
    where TParentViewNavigator : IViewNavigator
{
    private readonly IConnectionManager _connectionManager;
    private readonly IConnectionGroupFactory _connectionGroupFactory;

    protected IServersLoader ServersLoader { get; }
    protected ISettings Settings { get; }

    protected readonly SmartObservableCollection<ConnectionItemBase> Items = [];
    protected readonly SmartObservableCollection<ConnectionGroup> Groups = [];

    public CollectionViewSource GroupsCvs { get; }

    public bool HasItems => GroupsCvs.View.Any();

    protected ConnectionListViewModelBase(
        TParentViewNavigator parentViewNavigator,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IConnectionGroupFactory connectionGroupFactory,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, viewModelHelper)
    {
        Settings = settings;
        ServersLoader = serversLoader;
        _connectionManager = connectionManager;
        _connectionGroupFactory = connectionGroupFactory;

        GroupsCvs = new()
        {
            Source = Groups,
            IsSourceGrouped = true
        };
    }

    protected void ResetItems(IEnumerable<ConnectionItemBase> newItems)
    {
        Items.Reset(
            newItems.OrderBy(item => item.GroupType)
                    .ThenBy(item => item.FirstSortProperty)
                    .ThenBy(item => item.SecondSortProperty));
    }

    protected void ResetGroups()
    {
        Groups.Reset(
            Items.GroupBy(item => item.GroupType)
                 .OrderBy(group => group.Key)
                 .Select(group => _connectionGroupFactory.GetGroup(group.Key, group)));
    }

    protected void InvalidateActiveConnection()
    {
        if (IsActive)
        {
            ConnectionDetails? connectionDetails = _connectionManager.CurrentConnectionDetails;
            foreach (ConnectionItemBase item in Items)
            {
                item.InvalidateIsActiveConnection(connectionDetails);
            }
        }
    }

    protected virtual void InvalidateMaintenanceStates()
    {
    }

    protected void InvalidateRestrictions()
    {
        if (IsActive)
        {
            bool isPaidUser = Settings.VpnPlan.IsPaid;
            foreach (ConnectionItemBase item in Items)
            {
                item.InvalidateIsRestricted(isPaidUser);
            }
        }
    }
}