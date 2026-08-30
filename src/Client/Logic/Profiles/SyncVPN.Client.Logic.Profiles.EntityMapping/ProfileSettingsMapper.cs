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
using SyncVPN.Client.Settings.Contracts.Enums;
using SyncVPN.Common.Core.Networking;
using SyncVPN.EntityMapping.Contracts;

namespace SyncVPN.Client.Logic.Profiles.EntityMapping;

public class ProfileSettingsMapper : IMapper<IProfileSettings, SerializableProfileSettings>
{
    public SerializableProfileSettings Map(IProfileSettings leftEntity)
    {
        return leftEntity is null
            ? null
            : new SerializableProfileSettings()
            {
                VpnProtocol = (int)leftEntity.VpnProtocol,
                IsNetShieldEnabled = leftEntity.IsNetShieldEnabled,
                NetShieldMode = (int)leftEntity.NetShieldMode,
                IsPortForwardingEnabled = leftEntity.IsPortForwardingEnabled,
                NatType = (int)leftEntity.NatType,
            };
    }

    public IProfileSettings Map(SerializableProfileSettings rightEntity)
    {
        return rightEntity is null
            ? null
            : new ProfileSettings()
            {
                VpnProtocol = (VpnProtocol)rightEntity.VpnProtocol,
                IsNetShieldEnabled = rightEntity.IsNetShieldEnabled,
                NetShieldMode = (NetShieldMode)rightEntity.NetShieldMode,
                IsPortForwardingEnabled = rightEntity.IsPortForwardingEnabled,
                NatType = (NatType)rightEntity.NatType,
            };
    }
}