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

using SyncVPN.Common.Core.Networking;
using SyncVPN.EntityMapping.Contracts;

namespace SyncVPN.ProcessCommunication.EntityMapping.Vpn;

public class NetworkAddressMapper : IMapper<NetworkAddress, string>
{
    public string Map(NetworkAddress leftEntity)
    {
        return leftEntity.ToString();
    }

    public NetworkAddress Map(string rightEntity)
    {
        if (NetworkAddress.TryParse(rightEntity, out NetworkAddress address))
        {
            return address;
        }

        throw new ArgumentException($"IP address {rightEntity} is not a valid IPv4 or IPv6 address.");
    }
}