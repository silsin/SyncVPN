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

using NUnit.Framework;
using SyncVPN.UI.Tests.TestsHelper;
using SyncVPN.UI.Tests.ApiClient.TestEnv;

namespace SyncVPN.UI.Tests.TestBase;

// Setup dedicated for SLI tests to push all the metrics.
public class SliSetUp : BaseTest
{
    private LokiPusher _lokiPusher = new();

    [SetUp]
    public void SetUp()
    {
        SliHelper.Reset();
    }

    [TearDown]
    public void TestCleanup()
    {
        Cleanup();
        _lokiPusher.PushMetrics();
        _lokiPusher.PushAllLogs();
        SliHelper.Reset();
    }
}
