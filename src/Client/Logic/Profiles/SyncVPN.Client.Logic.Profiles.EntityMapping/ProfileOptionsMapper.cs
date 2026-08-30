/*
 * Copyright (c) 2024 Proton AG
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

using SyncVPN.Client.Logic.Profiles.Contracts.Models;
using SyncVPN.Client.Logic.Profiles.Contracts.SerializableEntities;
using SyncVPN.EntityMapping.Contracts;

namespace SyncVPN.Client.Logic.Profiles.EntityMapping;

public class ProfileOptionsMapper : IMapper<IProfileOptions, SerializableProfileOptions>
{
    public SerializableProfileOptions Map(IProfileOptions leftEntity)
    {
        return leftEntity is null
            ? null
            : new SerializableProfileOptions()
            {
                IsConnectAndGoEnabled = leftEntity.ConnectAndGo.IsEnabled,
                ConnectAndGoMode = leftEntity.ConnectAndGo.Mode,
                UsePrivateBrowsingMode = leftEntity.ConnectAndGo.UsePrivateBrowsingMode,
                ConnectAndGoUrl = leftEntity.ConnectAndGo.Url,
                ConnectAndGoAppPath = leftEntity.ConnectAndGo.AppPath
            };
    }

    public IProfileOptions Map(SerializableProfileOptions rightEntity)
    {
        return rightEntity is null
            ? null
            : new ProfileOptions()
            {
                ConnectAndGo = new ConnectAndGoOption()
                {
                    IsEnabled = rightEntity.IsConnectAndGoEnabled,
                    Mode = rightEntity.ConnectAndGoMode,
                    UsePrivateBrowsingMode = rightEntity.UsePrivateBrowsingMode,
                    Url = rightEntity.ConnectAndGoUrl,
                    AppPath = rightEntity.ConnectAndGoAppPath
                }
            };
    }
}