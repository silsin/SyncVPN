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

using SyncVPN.Client.Logic.Connection.Contracts.Models;

namespace SyncVPN.Client.Logic.Connection.Tests.Models.Intents.Locations;

[TestClass]
public class ServerLocationIntentTests
{
    [DataRow("CH#35", 35)]
    [DataRow("DE#53-TOR", 53)]
    [DataRow("DE-TOR-53", 0)]
    [DataRow("DE#TOR-53", 0)]
    [DataRow("CH#0", 0)]
    [DataRow("CH", 0)]
    [DataRow("CH#", 0)]
    [DataRow("#CH", 0)]
    [TestMethod]
    public void ServerLocationIntent_Number_ShouldBeValid(string name, int number)
    {
        ServerInfo server = ServerInfo.From("ID", name);

        Assert.AreEqual(number, server.GetNumber());
    }
}