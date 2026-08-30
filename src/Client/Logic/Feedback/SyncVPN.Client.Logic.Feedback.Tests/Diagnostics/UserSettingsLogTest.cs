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

using FluentAssertions;
using NSubstitute;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Feedback.Diagnostics.Logs;
using SyncVPN.Client.Settings.Contracts;

namespace SyncVPN.Client.Logic.Feedback.Tests.Diagnostics;

[TestClass]
public class UserSettingsLogTest : LogBaseTest
{
    private ISettings? _settings;
    private IUserAuthenticator? _userAuthenticator;

    [TestInitialize]
    public override void Initialize()
    {
        base.Initialize();

        _settings = Substitute.For<ISettings>();

        _userAuthenticator = Substitute.For<IUserAuthenticator>();
        _userAuthenticator.IsLoggedIn.Returns(true);
    }

    [TestCleanup]
    public override void Cleanup()
    {
        base.Cleanup();

        _settings = null;
        _userAuthenticator = null;
    }

    [TestMethod]
    public void ItShouldCreateLogFile()
    {
        // Act
        new UserSettingsLog(StaticConfig!, _settings!, _userAuthenticator!).Write();

        // Assert
        File.Exists(Path.Combine(TMP_PATH, "Settings.txt")).Should().BeTrue();
    }
}