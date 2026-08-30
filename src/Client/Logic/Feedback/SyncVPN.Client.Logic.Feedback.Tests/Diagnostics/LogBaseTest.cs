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

using NSubstitute;
using SyncVPN.Configurations.Contracts;

namespace SyncVPN.Client.Logic.Feedback.Tests.Diagnostics;

public class LogBaseTest
{
    protected const string TMP_PATH = "BugReporting\\Diagnostic\\Tmp";
    protected IStaticConfiguration? StaticConfig { get; private set; }

    [TestInitialize]
    public virtual void Initialize()
    {
        if (Directory.Exists(TMP_PATH))
        {
            Directory.Delete(TMP_PATH, true);
        }

        Directory.CreateDirectory(TMP_PATH);
        StaticConfig = CreateStaticConfiguration();
    }

    private IStaticConfiguration CreateStaticConfiguration()
    {
        IStaticConfiguration staticConfig = Substitute.For<IStaticConfiguration>();
        staticConfig.DiagnosticLogsFolder.Returns(TMP_PATH);
        return staticConfig;
    }

    [TestCleanup]
    public virtual void Cleanup()
    {
        StaticConfig = null;
    }
}