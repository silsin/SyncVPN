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
using SyncVPN.Configurations.Contracts;
using SyncVPN.Dns.Contracts;
using SyncVPN.Dns.Resolvers;
using SyncVPN.Dns.Tests.Mocks;

namespace SyncVPN.Dns.Tests.Resolvers;

[TestClass]
public class DnsOverHttpsResolverTest
    : DnsOverHttpsResolverTestBase<DnsOverHttpsResolver>
{
    private const string HOST = "api.syncvpn.com";

    public DnsOverHttpsResolverTest() : base(HOST)
    {
    }

    protected override DnsOverHttpsResolver CreateResolver(IConfiguration configuration,
        MockOfLogger logger, MockOfHttpClientFactory mockOfHttpClientFactory,
        IDnsOverHttpsProvidersManager dnsOverHttpsProvidersManager)
    {
        return new DnsOverHttpsResolver(configuration, logger,
            mockOfHttpClientFactory, dnsOverHttpsProvidersManager);
    }

    protected override void AssertCorrectResponse(DnsResponse response)
    {
        Assert.IsNotEmpty(response.IpAddresses);
    }
}