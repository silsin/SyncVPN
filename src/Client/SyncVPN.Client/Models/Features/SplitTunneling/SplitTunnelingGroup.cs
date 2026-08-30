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

using SyncVPN.Client.Contracts.Enums;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Extensions;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Localization.Extensions;

namespace SyncVPN.Client.Models.Features.SplitTunneling;

public partial class SplitTunnelingGroup : List<SplitTunnelingItemBase>
{
    public ILocalizationProvider Localizer { get; }

    public SplitTunnelingGroupType GroupType { get; }

    public int ItemsCount => this.Count();

    public string Header => Localizer.GetSplitTunnelingGroupName(GroupType, ItemsCount);

    public SplitTunnelingGroup(
        ILocalizationProvider localizer,
        SplitTunnelingGroupType groupType,
        IEnumerable<SplitTunnelingItemBase> items)
        : base(items)
    {
        Localizer = localizer;
        GroupType = groupType;
    }
}