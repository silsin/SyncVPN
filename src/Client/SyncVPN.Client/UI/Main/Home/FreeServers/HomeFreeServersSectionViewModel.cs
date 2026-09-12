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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Models.Connections;
using SyncVPN.Client.Services.FreeServers;

namespace SyncVPN.Client.UI.Main.Home.FreeServers;

// Home-page counterpart to the free-servers rows already shown in the Countries sidebar list
// (AllCountriesComponentViewModel) - same underlying IFreeServersCache and ILocationItemFactory, so
// both surfaces always agree, and reuses the same SyncVpnServerLocationItemTemplate/
// ConnectionItemsControl for identical row look and connect behavior. Kept as an additional, separate
// surface rather than a replacement for the Countries entry, since that one is still a guest's only
// content there. Also carries the search box that used to live over the map (HomeSearchBarComponent) -
// it now filters this list by server/city/country instead of panning the map to a country.
public partial class HomeFreeServersSectionViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IFreeServersCache _freeServersCache;
    private readonly ILocationItemFactory _locationItemFactory;
    private readonly List<ConnectionItemBase> _allServers = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ObservableCollection<ConnectionItemBase> Servers { get; } = [];

    public bool HasServers => _allServers.Count > 0;

    public HomeFreeServersSectionViewModel(
        IFreeServersCache freeServersCache,
        ILocationItemFactory locationItemFactory,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _freeServersCache = freeServersCache;
        _locationItemFactory = locationItemFactory;
    }

    public void Receive(ServerListChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateServers);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateServers();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    private void InvalidateServers()
    {
        _allServers.Clear();

        // This section is specifically the free-servers teaser (see class remarks) - the cache now also
        // holds Pro servers (for Countries sidebar use), so filter back down to the free subset here.
        foreach (ConnectionItemBase item in _freeServersCache.GetServers().Where(s => s.Free == 1).Select(_locationItemFactory.GetSyncVpnServer))
        {
            _allServers.Add(item);
        }

        OnPropertyChanged(nameof(HasServers));

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Servers.Clear();

        IEnumerable<ConnectionItemBase> filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allServers
            : _allServers.Where(MatchesSearch);

        foreach (ConnectionItemBase item in filtered)
        {
            Servers.Add(item);
        }
    }

    private bool MatchesSearch(ConnectionItemBase item)
    {
        return item.Header.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || item.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || (item.ToolTip?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false);
    }
}
