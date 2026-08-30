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

using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;
using SyncVPN.Client.Logic.Servers.Contracts.Models;

namespace SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.GatewayServers;

public class SingleGatewayServerLocationIntent : GatewayServerLocationIntentBase, ISingleLocationIntent
{
    public static SingleGatewayServerLocationIntent From(string gatewayName, GatewayServerInfo server)
        => new(SingleGatewayLocationIntent.From(gatewayName), server);

    public GatewayServerInfo Server { get; }

    public SingleGatewayServerLocationIntent(
        SingleGatewayLocationIntent gateway,
        GatewayServerInfo server)
        : base(gateway)
    {
        Server = server;
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return base.IsSameAs(intent)
            && intent is SingleGatewayServerLocationIntent gatewayServerIntent
            && Server == gatewayServerIntent.Server;
    }

    public override bool IsSupported(Server server)
    {
        return base.IsSupported(server)
            && server.Id == Server.Id;
    }

    public override string ToString()
    {
        return $"{base.ToString()} - Server {Server.Name}";
    }
}