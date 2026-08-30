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

using SyncVPN.Common.Legacy.KillSwitch;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace SyncVPN.ProcessCommunication.EntityMapping.Vpn;

public class LegacyKillSwitchModeMapper : IMapper<KillSwitchMode, KillSwitchModeIpcEntity>
{
    public KillSwitchModeIpcEntity Map(KillSwitchMode leftEntity)
    {
        return leftEntity switch
        {
            KillSwitchMode.Off => KillSwitchModeIpcEntity.Off,
            KillSwitchMode.Soft => KillSwitchModeIpcEntity.Soft,
            KillSwitchMode.Hard => KillSwitchModeIpcEntity.Hard,
            _ => throw new NotImplementedException("KillSwitchMode has an unknown value.")
        };
    }

    public KillSwitchMode Map(KillSwitchModeIpcEntity rightEntity)
    {
        return rightEntity switch
        {
            KillSwitchModeIpcEntity.Off => KillSwitchMode.Off,
            KillSwitchModeIpcEntity.Soft => KillSwitchMode.Soft,
            KillSwitchModeIpcEntity.Hard => KillSwitchMode.Hard,
            _ => throw new NotImplementedException("KillSwitchMode has an unknown value.")
        };
    }
}