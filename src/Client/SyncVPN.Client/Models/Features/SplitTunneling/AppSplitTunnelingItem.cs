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

using Microsoft.UI.Xaml.Media;
using SyncVPN.Client.Contracts.Enums;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Settings.Contracts.Models;

namespace SyncVPN.Client.Models.Features.SplitTunneling;

public class AppSplitTunnelingItem : SplitTunnelingItemBase
{
    public SplitTunnelingApp App { get; }

    public string Name { get; }

    public ImageSource? Icon { get; }

    public AppSplitTunnelingItem(
        ILocalizationProvider localizer,
        SplitTunnelingGroupType groupType,
        SplitTunnelingApp app,
        string name,
        ImageSource? icon)
        : base(localizer, groupType)
    {
        App = app;
        Name = name;
        Icon = icon;
    }
}
