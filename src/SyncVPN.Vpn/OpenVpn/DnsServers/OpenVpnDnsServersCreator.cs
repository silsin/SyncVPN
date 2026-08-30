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

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SyncVPN.Common.Core.Networking;
using SyncVPN.Vpn.Gateways;

namespace SyncVPN.Vpn.OpenVpn.DnsServers;

public class OpenVpnDnsServersCreator : IOpenVpnDnsServersCreator
{
    private readonly IDnsServerCache _dnsServerCache;

    public OpenVpnDnsServersCreator(IDnsServerCache dnsServerCache)
    {
        _dnsServerCache = dnsServerCache;
    }

    public string GetDnsServers(IReadOnlyCollection<string> customDns, bool isIpv6Supported)
    {
        List<string> dnsAddresses = [];

        if (customDns.Count > 0)
        {
            foreach (string dns in customDns)
            {
                if (NetworkAddress.TryParse(dns, out NetworkAddress networkAddress) &&
                    networkAddress.IsIpV4 || networkAddress.IsIpV6 && isIpv6Supported)
                {
                    dnsAddresses.Add(dns);
                }
            }
        }

        // Append the default DNS servers as a fallback for the ones provided by the user
        List<IPAddress> defaultDnsServers = _dnsServerCache.Get();

        foreach(IPAddress defaultDnsServer in defaultDnsServers)
        {
            if (defaultDnsServer.AddressFamily == AddressFamily.InterNetwork || isIpv6Supported)
            {
                dnsAddresses.Add(defaultDnsServer.ToString());
            }
        }

        return string.Join(",", dnsAddresses);
    }
}
