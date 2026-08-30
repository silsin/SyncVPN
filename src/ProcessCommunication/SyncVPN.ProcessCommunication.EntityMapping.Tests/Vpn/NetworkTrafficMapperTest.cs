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
using SyncVPN.Common.Core.Networking;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;
using SyncVPN.ProcessCommunication.EntityMapping.Vpn;

namespace SyncVPN.ProcessCommunication.EntityMapping.Tests.Vpn;

[TestClass]
public class NetworkTrafficMapperTest
{
    private NetworkTrafficMapper _mapper;

    [TestInitialize]
    public void Initialize()
    {
        _mapper = new();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _mapper = null;
    }

    [TestMethod]
    public void TestMapLeftToRight_WhenZero()
    {
        NetworkTraffic entityToTest = NetworkTraffic.Zero;

        NetworkTrafficIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(0u, result.BytesDownloaded);
        Assert.AreEqual(0u, result.BytesUploaded);
    }

    [TestMethod]
    public void TestMapLeftToRight()
    {
        NetworkTraffic entityToTest = new((ulong)DateTime.UtcNow.Ticks, (ulong)DateTime.UtcNow.Millisecond);

        NetworkTrafficIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.BytesDownloaded, result.BytesDownloaded);
        Assert.AreEqual(entityToTest.BytesUploaded, result.BytesUploaded);
    }

    [TestMethod]
    public void TestMapRightToLeft_WhenNull()
    {
        NetworkTrafficIpcEntity entityToTest = null;

        NetworkTraffic result = _mapper.Map(entityToTest);

        Assert.AreEqual(0u, result.BytesDownloaded);
        Assert.AreEqual(0u, result.BytesUploaded);
    }

    [TestMethod]
    public void TestMapRightToLeft()
    {
        NetworkTrafficIpcEntity entityToTest = new()
        {
            BytesDownloaded = (ulong)DateTime.UtcNow.Ticks,
            BytesUploaded = (ulong)DateTime.UtcNow.Millisecond,
        };

        NetworkTraffic result = _mapper.Map(entityToTest);

        Assert.AreEqual(entityToTest.BytesDownloaded, result.BytesDownloaded);
        Assert.AreEqual(entityToTest.BytesUploaded, result.BytesUploaded);
    }
}