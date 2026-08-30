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

using SyncVPN.Common.Core.Dns;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Dns;

namespace SyncVPN.ProcessCommunication.EntityMapping.Dns;

public class DnsBlockModeMapper : IMapper<DnsBlockMode, DnsBlockModeIpcEntity>
{
    public DnsBlockModeIpcEntity Map(DnsBlockMode leftEntity)
    {
        return leftEntity switch
        {
            DnsBlockMode.Nrpt => DnsBlockModeIpcEntity.Nrpt,
            DnsBlockMode.Callout => DnsBlockModeIpcEntity.Callout,
            DnsBlockMode.Disabled => DnsBlockModeIpcEntity.Disabled,
            _ => throw new NotImplementedException("DnsBlockMode has an unknown value.")
        };
    }

    public DnsBlockMode Map(DnsBlockModeIpcEntity rightEntity)
    {
        return rightEntity switch
        {
            DnsBlockModeIpcEntity.Nrpt => DnsBlockMode.Nrpt,
            DnsBlockModeIpcEntity.Callout => DnsBlockMode.Callout,
            DnsBlockModeIpcEntity.Disabled => DnsBlockMode.Disabled,
            _ => throw new NotImplementedException("DnsBlockMode has an unknown value.")
        };
    }
}