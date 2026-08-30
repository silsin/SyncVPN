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
using SyncVPN.Crypto.Contracts;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Crypto;
using SyncVPN.ProcessCommunication.EntityMapping.Crypto;

namespace SyncVPN.ProcessCommunication.EntityMapping.Tests.Crypto;

[TestClass]
public class SecretKeyMapperTest : KeyMapperTestBase<SecretKey, SecretKeyIpcEntity>
{
    protected override IMapper<SecretKey, SecretKeyIpcEntity> CreateKeyMapper()
    {
        return new SecretKeyMapper();
    }

    protected override SecretKey CreateKey(string base64, KeyAlgorithm algorithm)
    {
        return new(base64, algorithm);
    }

    protected override string CreateExpectedPem(string base64)
    {
        return $"-----BEGIN PRIVATE KEY-----\r\n{base64}\r\n-----END PRIVATE KEY-----";
    }
}