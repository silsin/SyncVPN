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

using SyncVPN.Logging.Contracts;
using SyncVPN.Client.Core.Services.Mapping;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Core.Services.Navigation.Bases;
using SyncVPN.Client.UI.Main.Sidebar.Connections;
using SyncVPN.Client.UI.Main.Sidebar.Search;
using SyncVPN.Client.Common.Dispatching;

namespace SyncVPN.Client.Services.Navigation;

public class SidebarViewNavigator : ViewNavigatorBase, ISidebarViewNavigator
{
    public SidebarViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher)
        : base(logger, pageViewMapper, uiThreadDispatcher)
    { }

    public Task<bool> NavigateToConnectionsViewAsync()
    {
        return NavigateToAsync<ConnectionsPageViewModel>();
    }

    public Task<bool> NavigateToSearchViewAsync()
    {
        return NavigateToAsync<SearchResultsPageViewModel>();
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return NavigateToConnectionsViewAsync();
    }
}