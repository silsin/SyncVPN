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

using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;
using SyncVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using SyncVPN.Client.Logic.Connection.EntityMapping.Extensions;
using SyncVPN.EntityMapping.Contracts;

namespace SyncVPN.Client.Logic.Connection.EntityMapping.LocationIntents;

public class MultiGatewayLocationIntentMapper : IMapper<MultiGatewayLocationIntent, SerializableLocationIntent>
{
    public SerializableLocationIntent Map(MultiGatewayLocationIntent leftEntity)
    {
        return leftEntity is null
            ? null
            : new SerializableLocationIntent()
            {
                TypeName = nameof(MultiGatewayLocationIntent),
                Strategy = leftEntity.Strategy,
                GatewayNames = leftEntity.GatewayNames.ToList(),
            };
    }

    public MultiGatewayLocationIntent Map(SerializableLocationIntent rightEntity)
    {
        SelectionStrategy strategy = rightEntity.GetSelectionStrategy();

        return rightEntity is null
            ? null
            : MultiGatewayLocationIntent.From(
                gatewayNames: rightEntity.GatewayNames ?? [],
                strategy: strategy);
    }
}