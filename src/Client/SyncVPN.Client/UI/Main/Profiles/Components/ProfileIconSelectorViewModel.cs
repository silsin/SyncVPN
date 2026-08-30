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
using SyncVPN.Client.Common.Enums;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Logic.Profiles.Contracts.Models;
using SyncVPN.Client.UI.Main.Profiles.Contracts;

namespace SyncVPN.Client.UI.Main.Profiles.Components;

public partial class ProfileIconSelectorViewModel : ViewModelBase, IProfileIconSelector
{
    private IProfileIcon _originalProfileIcon = ProfileIcon.Default;

    [ObservableProperty]
    private ProfileCategory _selectedCategory;

    [ObservableProperty]
    private ProfileColor _selectedColor;

    public ProfileIconSelectorViewModel(
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    { }

    public ProfileCategory GetProfileCategory()
    {
        return SelectedCategory;
    }

    public IProfileIcon GetProfileIcon()
    {
        return new ProfileIcon()
        {
            Category = SelectedCategory,
            Color = SelectedColor
        };
    }

    public void SetProfileIcon(IProfileIcon icon)
    {
        _originalProfileIcon = icon ?? ProfileIcon.Default;

        SelectedCategory = _originalProfileIcon.Category;
        SelectedColor = _originalProfileIcon.Color;
    }

    public bool HasChanged()
    {
        return _originalProfileIcon.Category != SelectedCategory
            || _originalProfileIcon.Color != SelectedColor;
    }

    public bool IsReconnectionRequired()
    {
        return false;
    }
}