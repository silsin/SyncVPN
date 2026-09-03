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

using CommunityToolkit.Mvvm.ComponentModel;
using SyncVPN.Client.Common.UI.Controls.Map;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.UI.Main.Map;

namespace SyncVPN.Client.UI.Main.Home.SearchBar;

public partial class HomeSearchBarComponentViewModel : ViewModelBase
{
    private readonly MapComponentViewModel _mapComponentViewModel;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public HomeSearchBarComponentViewModel(
        MapComponentViewModel mapComponentViewModel,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _mapComponentViewModel = mapComponentViewModel;
    }

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        Country? match = _mapComponentViewModel.Countries
            .FirstOrDefault(c => c.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            _mapComponentViewModel.CurrentCountry = match;
        }
    }
}
