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
using System.Net;
using System.Net.Sockets;
using SyncVPN.Client.Settings.Contracts.Models;
using SyncVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace SyncVPN.StatisticalEvents.Dimensions.Extensions;

public static class SettingsDimensionExtensions
{
    public static string GetFirstActiveDnsFamily(this List<CustomDnsServer>? customDnsServers)
    {
        if (customDnsServers == null || customDnsServers.Count == 0)
        {
            return DimensionMapperBase.NOT_AVAILABLE;
        }

        CustomDnsServer? firstActive = customDnsServers.FirstOrDefault(dns => dns.IsActive);
        
        if (firstActive == null || !IPAddress.TryParse(firstActive.Value.IpAddress, out IPAddress? ipAddress))
        {
            return DimensionMapperBase.NOT_AVAILABLE;
        }

        return ipAddress.AddressFamily switch
        {
            AddressFamily.InterNetwork => "ipv4",
            AddressFamily.InterNetworkV6 => "ipv6",
            _ => DimensionMapperBase.NOT_AVAILABLE
        };
    }
}