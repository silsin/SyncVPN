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

using System.Collections.ObjectModel;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.UI.Main.Features.NetShield;
using SyncVPN.Client.UI.Main.Settings;
using SyncVPN.Client.UI.Main.Widgets.Contracts;

namespace SyncVPN.Client.UI.Main.Widgets;

public partial class SideWidgetsHostComponentViewModel : ViewModelBase
{
    public ObservableCollection<ISideHeaderWidget> HeaderWidgets { get; }

    public ObservableCollection<ISideFooterWidget> FooterWidgets { get; }

    public bool HasHeaderAndFooterWidgets => HeaderWidgets.Count > 0 && FooterWidgets.Count > 0;

    public SideWidgetsHostComponentViewModel(
        IEnumerable<ISideHeaderWidget> headerWidgets,
        IEnumerable<ISideFooterWidget> footerWidgets,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        // NetShield and Settings already have their own entries in the sidebar's main
        // navigation list, so they're excluded here to avoid showing them twice.
        HeaderWidgets = new(headerWidgets.Where(w => w is not NetShieldWidgetViewModel).OrderBy(p => p.SortIndex));
        FooterWidgets = new(footerWidgets.Where(w => w is not SettingsWidgetViewModel).OrderBy(p => p.SortIndex));
    }
}