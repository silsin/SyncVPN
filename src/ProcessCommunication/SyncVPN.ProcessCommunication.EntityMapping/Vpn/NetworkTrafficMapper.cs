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

using SyncVPN.Common.Core.Networking;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace SyncVPN.ProcessCommunication.EntityMapping.Vpn;

public class NetworkTrafficMapper : IMapper<NetworkTraffic, NetworkTrafficIpcEntity>
{
    public NetworkTrafficIpcEntity Map(NetworkTraffic leftEntity)
    {
        return new()
        {
            BytesDownloaded = leftEntity.BytesDownloaded,
            BytesUploaded = leftEntity.BytesUploaded
        };
    }

    public NetworkTraffic Map(NetworkTrafficIpcEntity rightEntity)
    {
        return rightEntity is null
            ? NetworkTraffic.Zero
            : new(bytesDownloaded: rightEntity.BytesDownloaded, bytesUploaded: rightEntity.BytesUploaded);
    }
}