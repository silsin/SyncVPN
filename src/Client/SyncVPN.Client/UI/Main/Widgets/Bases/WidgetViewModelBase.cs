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
using SyncVPN.Client.UI.Main.Widgets.Contracts;

namespace SyncVPN.Client.UI.Main.Widgets.Bases;

public abstract partial class WidgetViewModelBase : ActivatableViewModelBase, IWidget
{
    public abstract int SortIndex { get; }

    public abstract string Header { get; }

    public virtual bool IsAvailable => true;

    protected WidgetViewModelBase(IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    { }

    [RelayCommand]
    public abstract Task<bool> InvokeAsync();

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Header));
    }
}