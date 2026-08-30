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

using SyncVPN.Common.Legacy.PortForwarding;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.PortForwarding;

namespace SyncVPN.ProcessCommunication.EntityMapping.PortForwarding;

public class TemporaryMappedPortMapper : IMapper<TemporaryMappedPort, TemporaryMappedPortIpcEntity>
{
    public TemporaryMappedPortIpcEntity Map(TemporaryMappedPort leftEntity)
    {
        return leftEntity?.MappedPort is null
            ? null
            : new TemporaryMappedPortIpcEntity()
            {
                InternalPort = leftEntity.MappedPort.InternalPort,
                ExternalPort = leftEntity.MappedPort.ExternalPort,
                Lifetime = leftEntity.Lifetime,
                ExpirationDateUtc = leftEntity.ExpirationDateUtc,
            };
    }

    public TemporaryMappedPort Map(TemporaryMappedPortIpcEntity rightEntity)
    {
        return rightEntity is null
            ? null
            : new TemporaryMappedPort()
            {
                MappedPort = new MappedPort(internalPort: rightEntity.InternalPort, externalPort: rightEntity.ExternalPort),
                Lifetime = rightEntity.Lifetime,
                ExpirationDateUtc = rightEntity.ExpirationDateUtc,
            };
    }
}