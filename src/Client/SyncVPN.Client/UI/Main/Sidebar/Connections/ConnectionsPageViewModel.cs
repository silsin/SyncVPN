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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Services.Mapping;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;

namespace SyncVPN.Client.UI.Main.Sidebar.Connections;

public partial class ConnectionsPageViewModel : PageViewModelBase<ISidebarViewNavigator, IConnectionsViewNavigator>
{
    private readonly IPageViewMapper _pageViewMapper;

    [ObservableProperty]
    private IConnectionPage? _selectedConnectionPage;

    public ObservableCollection<IConnectionPage> ConnectionPages { get; }

    public ConnectionsPageViewModel(
        ISidebarViewNavigator parentViewNavigator,
        IConnectionsViewNavigator childViewNavigator,
        IEnumerable<IConnectionPage> connectionPages,
        IPageViewMapper pageViewMapper,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, childViewNavigator, viewModelHelper)
    {
        _pageViewMapper = pageViewMapper;

        ConnectionPages = new(connectionPages.OrderBy(p => p.SortIndex));
    }

    protected override void OnChildNavigation(NavigationEventArgs e)
    {
        base.OnChildNavigation(e);

        Type connectionPageType = _pageViewMapper.GetViewModelType(e.SourcePageType);

        SelectedConnectionPage = ConnectionPages.FirstOrDefault(p => p.GetType().IsAssignableFrom(connectionPageType));
    }

    partial void OnSelectedConnectionPageChanged(IConnectionPage? value)
    {
        if (value != null && value.IsAvailable)
        {
            value.InvokeAsync();
        }
    }
}