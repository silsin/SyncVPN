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

using SyncVPN.Api.Contracts.Geographical;
using SyncVPN.Api.Contracts.Servers;
using SyncVPN.Client.Logic.Servers.Contracts.Enums;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.EntityMapping.Contracts;

namespace SyncVPN.Client.Logic.Servers.Mappers;

public class LogicalServerMapper : IMapper<LogicalServerResponse, Server>
{
    private readonly IEntityMapper _entityMapper;

    public LogicalServerMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public Server Map(LogicalServerResponse leftEntity)
    {
        return leftEntity is null
            ? null
            : new Server
            {
                Id = leftEntity.Id,
                Name = leftEntity.Name,
                City = leftEntity.City,
                State = leftEntity.State,
                EntryCountry = leftEntity.EntryCountry,
                ExitCountry = leftEntity.ExitCountry,
                HostCountry = leftEntity.HostCountry,
                Domain = leftEntity.Domain,
                Status = leftEntity.Status,
                Tier = (ServerTiers)leftEntity.Tier,
                Features = (ServerFeatures)leftEntity.Features,
                Load = leftEntity.Load,
                Score = leftEntity.Score,
                Servers = _entityMapper.Map<PhysicalServerResponse, PhysicalServer>(leftEntity.Servers),
                IsVirtual = !string.IsNullOrEmpty(leftEntity.HostCountry),
                GatewayName = leftEntity.GatewayName,
                StatusReference = _entityMapper.Map<StatusReferenceResponse, StatusReference>(leftEntity.StatusReference),
                EntryLocation = _entityMapper.Map<ServerLocationResponse, GeoLocation>(leftEntity.EntryLocation),
                ExitLocation = _entityMapper.Map<ServerLocationResponse, GeoLocation>(leftEntity.ExitLocation),
            };
    }

    public LogicalServerResponse Map(Server rightEntity)
    {
        throw new NotImplementedException("We don't need to map to API responses.");
    }
}