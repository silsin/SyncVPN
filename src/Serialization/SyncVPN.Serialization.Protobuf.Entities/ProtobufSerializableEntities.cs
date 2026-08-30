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

using SyncVPN.Client.Files.Contracts.Images;
using SyncVPN.Client.Logic.Announcements.Contracts.Entities;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;
using SyncVPN.Client.Logic.Profiles.Contracts.SerializableEntities;
using SyncVPN.Client.Logic.Recents.Contracts.SerializableEntities;
using SyncVPN.Client.Logic.Servers.Contracts.Enums;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Common.Core.Geographical;
using SyncVPN.Common.Core.Networking;
using SyncVPN.Common.Core.StatisticalEvents;
using SyncVPN.Serialization.Contracts;
using SyncVPN.StatisticalEvents.Contracts.Models;

namespace SyncVPN.Serialization.Protobuf.Entities;

public class ProtobufSerializableEntities : IProtobufSerializableEntities
{
    public List<Type> Types { get; } = CreateTypeList().ToList();

    private static IEnumerable<Type> CreateTypeList()
    {
        yield return typeof(ServersFile);
        yield return typeof(PhysicalServer);
        yield return typeof(Server);
        yield return typeof(StatusReference);
        yield return typeof(GeoLocation);
        yield return typeof(ServerFeatures);
        yield return typeof(ServerTiers);
        yield return typeof(VpnProtocol);
        yield return typeof(DeviceLocation);
        yield return typeof(SelectionStrategy);
        yield return typeof(ServerInfo);
        yield return typeof(GatewayServerInfo);

        yield return typeof(LocationNamesFile);
        yield return typeof(LocationNamesCache);

        yield return typeof(SerializableConnectionIntent);
        yield return typeof(SerializableFeatureIntent);
        yield return typeof(SerializableLocationIntent);
        yield return typeof(SerializableRecentConnection);

        yield return typeof(CachedImage);

        yield return typeof(Announcement);
        yield return typeof(AnnouncementType);
        yield return typeof(FullScreenImage);
        yield return typeof(Panel);
        yield return typeof(PanelButton);
        yield return typeof(PanelFeature);

        yield return typeof(SerializableProfile);
        yield return typeof(SerializableProfileIcon);
        yield return typeof(SerializableProfileSettings);
        yield return typeof(SerializableProfileOptions);

        yield return typeof(StatisticalEventsFile);
        yield return typeof(StatisticalEvent);
    }
}