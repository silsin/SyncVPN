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

using SyncVPN.Common.Core.Vpn;
using SyncVPN.Common.Legacy.Vpn;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace SyncVPN.ProcessCommunication.EntityMapping.Vpn;

public class ConnectionDetailsMapper : IMapper<ConnectionDetails, ConnectionDetailsIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public ConnectionDetailsMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public ConnectionDetailsIpcEntity Map(ConnectionDetails leftEntity)
    {
        return leftEntity is null
            ? null
            : new ConnectionDetailsIpcEntity()
            {
                ClientIpAddress = leftEntity.ClientIpAddress,
                ClientCountryIsoCode = leftEntity.ClientCountryIsoCode,
                ServerIpAddress = _entityMapper.Map<IpAddressInfo, VpnServerAddressIpcEntity>(leftEntity.ServerIpAddress),
            };
    }

    public ConnectionDetails Map(ConnectionDetailsIpcEntity rightEntity)
    {
        return rightEntity is null
            ? null
            : new ConnectionDetails
            {
                ClientIpAddress = rightEntity.ClientIpAddress,
                ClientCountryIsoCode = rightEntity.ClientCountryIsoCode,
                ServerIpAddress = _entityMapper.Map<VpnServerAddressIpcEntity, IpAddressInfo>(rightEntity.ServerIpAddress),
            };
    }
}