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

using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.SyncVpnServers;
using SyncVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using SyncVPN.EntityMapping.Contracts;

namespace SyncVPN.Client.Logic.Connection.EntityMapping.LocationIntents;

public class SyncVpnServerLocationIntentMapper : IMapper<SyncVpnServerLocationIntent, SerializableLocationIntent>
{
    public SerializableLocationIntent Map(SyncVpnServerLocationIntent leftEntity)
    {
        return leftEntity is null
            ? null
            : new SerializableLocationIntent()
            {
                TypeName = nameof(SyncVpnServerLocationIntent),
                Server = ServerInfo.From(leftEntity.ServerId.ToString(), leftEntity.ServerName),
                Protocol = leftEntity.Protocol,
                Transport = leftEntity.Transport,
                IsForPaidUsersOnly = leftEntity.IsForPaidUsersOnly,
            };
    }

    public SyncVpnServerLocationIntent Map(SerializableLocationIntent rightEntity)
    {
        if (rightEntity?.Server is not { } server || !long.TryParse(server.Id, out long serverId))
        {
            return null;
        }

        return new SyncVpnServerLocationIntent(
            serverId,
            server.Name,
            rightEntity.Protocol ?? string.Empty,
            rightEntity.Transport,
            rightEntity.IsForPaidUsersOnly ?? false);
    }
}
