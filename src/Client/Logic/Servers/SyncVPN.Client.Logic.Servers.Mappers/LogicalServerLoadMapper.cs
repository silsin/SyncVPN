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

using SyncVPN.Api.Contracts.Servers;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.EntityMapping.Contracts;

namespace SyncVPN.Client.Logic.Servers.Mappers;

public class LogicalServerLoadMapper : IMapper<LogicalServerResponse, ServerLoad>
{
    public ServerLoad Map(LogicalServerResponse leftEntity)
    {
        return leftEntity is null
            ? null
            : new ServerLoad
            {
                Id = leftEntity.Id,
                Status = leftEntity.Status,
                Load = leftEntity.Load,
                Score = leftEntity.Score,
            };
    }

    public LogicalServerResponse Map(ServerLoad rightEntity)
    {
        throw new NotImplementedException("We don't need to map to API responses.");
    }
}