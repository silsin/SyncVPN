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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncVPN.Common.Core.Dns;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Dns;
using SyncVPN.ProcessCommunication.EntityMapping.Dns;
using SyncVPN.ProcessCommunication.EntityMapping.Tests.Common;

namespace SyncVPN.ProcessCommunication.EntityMapping.Tests.Dns;

[TestClass]
public class DnsBlockModeMapperTest : EnumMapperTestBase<DnsBlockMode, DnsBlockModeIpcEntity>
{
    protected override IMapper<DnsBlockMode, DnsBlockModeIpcEntity> CreateMapper()
    {
        return new DnsBlockModeMapper();
    }
}