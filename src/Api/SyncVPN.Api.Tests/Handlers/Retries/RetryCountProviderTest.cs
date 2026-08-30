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

using System.Net.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SyncVPN.Api.Handlers.Retries;
using SyncVPN.Common.Legacy.OS.Net.Http;
using SyncVPN.Configurations.Contracts;

namespace SyncVPN.Api.Tests.Handlers.Retries;

[TestClass]
public class RetryCountProviderTest
{
    [TestMethod]
    public void ItShouldUseDefaultRetryCount()
    {
        // Arrange
        int retryCount = 3;
        IConfiguration config = Substitute.For<IConfiguration>();
        config.ApiRetries.Returns(retryCount);
        RetryCountProvider sut = new(config);

        // Assert
        sut.GetRetryCount(new HttpRequestMessage()).Should().Be(retryCount);
    }

    [TestMethod]
    public void ItShouldUseCustomRetryCount()
    {
        // Arrange
        IConfiguration config = Substitute.For<IConfiguration>();
        config.ApiRetries.Returns(3);
        RetryCountProvider sut = new(config);
        HttpRequestMessage request = new();
        int retryCount = 10;
        request.SetRetryCount(retryCount);

        // Assert
        sut.GetRetryCount(request).Should().Be(retryCount);
    }
}