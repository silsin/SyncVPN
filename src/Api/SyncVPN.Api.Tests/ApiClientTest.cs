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

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SyncVPN.Api.BackendSelection;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.Contracts.Servers;
using SyncVPN.Api.V2.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Common.Core.Geographical;
using SyncVPN.Configurations.Contracts;
using SyncVPN.Logging.Contracts;
using RichardSzalay.MockHttp;

namespace SyncVPN.Api.Tests;

[TestClass]
public class ApiClientTest
{
    private ILogger _logger;
    private ISettings _appSettings;
    private IApiAppVersion _appVersion;
    private IApiHttpClientFactory _apiHttpClientFactory;
    private IApiClient _apiClient;
    private readonly MockHttpMessageHandler _fakeHttpMessageHandler = new();

    [TestInitialize]
    public void TestInitialize()
    {
        _logger = Substitute.For<ILogger>();

        _appVersion = Substitute.For<IApiAppVersion>();
        _appVersion.AppVersion.Returns(string.Empty);
        _appVersion.UserAgent.Returns("User agent");

        _appSettings = Substitute.For<ISettings>();
        _appSettings.AccessToken.Returns(string.Empty);
        _appSettings.UniqueSessionId.Returns(string.Empty);

        HttpClient httpClient = _fakeHttpMessageHandler.ToHttpClient();
        httpClient.BaseAddress = new("http://127.0.0.1");

        _apiHttpClientFactory = Substitute.For<IApiHttpClientFactory>();
        _apiHttpClientFactory.GetApiHttpClientWithoutCache().Returns(httpClient);
        _apiHttpClientFactory.GetApiHttpClientWithCache().Returns(httpClient);

        IConfiguration config = Substitute.For<IConfiguration>();

        IBackendModeProvider backendModeProvider = Substitute.For<IBackendModeProvider>();
        ISyncVpnApiClient syncVpnApiClient = Substitute.For<ISyncVpnApiClient>();

        _apiClient = new ApiClient(_apiHttpClientFactory, _logger, _appVersion, _appSettings, config, backendModeProvider, syncVpnApiClient);
    }

    [TestMethod]
    public async Task ServerListDownloadedAsync()
    {
        _fakeHttpMessageHandler.When("*").Respond(_ => new()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{'Code' : '1000', 'Servers': []}")
        });

        DeviceLocation deviceLocation = new() { CountryCode = "CH", IpAddress = "127.0.0.0" };
        ApiResponseResult<ServersResponse> response = await _apiClient.GetServersAsync(deviceLocation);

        response.Success.Should().BeTrue();
    }
}