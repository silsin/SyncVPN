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

using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Common.Core.Geographical;
using SyncVPN.Common.Core.Networking;

namespace SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;

public interface IConnectionIntent
{
    ILocationIntent Location { get; }

    IFeatureIntent? Feature { get; }

    bool IsSameAs(IConnectionIntent? intent);

    IOrderedEnumerable<Server> FilterAndSortServers(IEnumerable<Server> servers, DeviceLocation? deviceLocation, IList<VpnProtocol> preferredProtocols, bool isPortForwardingEnabled);

    bool IsSupported(Server server, DeviceLocation? deviceLocation);

    bool HasNoServers(IEnumerable<Server> servers, DeviceLocation? deviceLocation);

    bool AreAllServersUnderMaintenance(IEnumerable<Server> servers, DeviceLocation? deviceLocation);

    bool IsPortForwardingSupported();
}