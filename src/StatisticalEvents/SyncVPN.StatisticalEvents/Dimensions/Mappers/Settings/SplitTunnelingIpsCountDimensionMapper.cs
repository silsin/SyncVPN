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

using System.Collections.Generic;
using System.Linq;
using SyncVPN.Client.Settings.Contracts.Enums;
using SyncVPN.Client.Settings.Contracts.Models;
using SyncVPN.StatisticalEvents.Dimensions.Constants;
using SyncVPN.StatisticalEvents.Dimensions.Extensions;
using SyncVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace SyncVPN.StatisticalEvents.Dimensions.Mappers.Settings;

public class SplitTunnelingIpsCountDimensionMapper : DimensionMapperBase, ISplitTunnelingIpsCountDimensionMapper
{
    public string Map(int count)
    {
        return count.ToSplitTunnelingCountDimension();
    }

    public string Map(SplitTunnelingMode splitTunnelingMode, List<SplitTunnelingIpAddress>? standardIps, List<SplitTunnelingIpAddress>? inverseIps)
    {
        int activeCount = splitTunnelingMode switch
        {
            SplitTunnelingMode.Standard => standardIps?.Count(ip => ip.IsActive) ?? 0,
            SplitTunnelingMode.Inverse => inverseIps?.Count(ip => ip.IsActive) ?? 0,
            _ => 0
        };

        return Map(activeCount);
    }
}