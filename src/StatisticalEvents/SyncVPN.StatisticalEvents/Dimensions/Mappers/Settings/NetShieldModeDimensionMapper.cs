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

using SyncVPN.Client.Settings.Contracts.Enums;
using SyncVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace SyncVPN.StatisticalEvents.Dimensions.Mappers.Settings;

public class NetShieldModeDimensionMapper : DimensionMapperBase, INetShieldModeDimensionMapper
{
    private const string NETSHIELD_OFF = "off";
    private const string MALWARE_ONLY = "malware";
    private const string ADS_TRACKERS_AND_MALWARE = "ads_trackers_and_malware";
    private const string ADS_TRACKERS_MALWARE_AND_ADULT_CONTENT = "ads_trackers_malware_and_adult_content";

    public string Map(bool isNetShieldEnabled, NetShieldMode netShieldMode)
    {
        if (!isNetShieldEnabled)
        {
            return NETSHIELD_OFF;
        }

        return netShieldMode switch
        {
            NetShieldMode.BlockMalwareOnly => MALWARE_ONLY,
            NetShieldMode.BlockAdsMalwareTrackers => ADS_TRACKERS_AND_MALWARE,
            NetShieldMode.BlockAdsMalwareTrackersAdultContent => ADS_TRACKERS_MALWARE_AND_ADULT_CONTENT,
            _ => NOT_AVAILABLE
        };
    }
}
