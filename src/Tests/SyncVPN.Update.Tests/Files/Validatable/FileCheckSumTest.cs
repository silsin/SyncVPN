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

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncVPN.Tests.Common;
using SyncVPN.Update.Files.Validatable;

namespace SyncVPN.Update.Tests.Files.Validatable
{
    [TestClass]
    public class FileCheckSumTest
    {
        [TestMethod]
        public async Task Value_ShouldBe_SHA512()
        {
            string filename = TestConfig.GetFolderPath("SyncVPN_win_v1.5.1.exe");
            FileCheckSum fileCheckSum = new(filename);

            string result = await fileCheckSum.Value();

            result.Should().Be("c011146ae24f5a49ef86ff6199ec0bd42223b408e1dce3ffef9a2ef4b9c1806b1c823ce427d7473378b7d8c427ba6cb3701320740523ad79fc9ec8cfeb907875");
        }
    }
}