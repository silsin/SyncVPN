/*
 * Copyright (c) 2026 Proton AG
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

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncVPN.Vpn.WireGuard;

namespace SyncVPN.Vpn.Tests.WireGuard;

[TestClass]
public class WireGuardConfigDnsPatcherTest
{
    private const string SERVER_ISSUED_CONFIG =
        "[Interface]\nPrivateKey = CLIENT_PRIVATE_KEY\nAddress = 10.70.0.22/32\nDNS = 1.1.1.1, 1.0.0.1\nMTU = 1420\n\n" +
        "[Peer]\nPublicKey = SERVER_PUBLIC_KEY\nAllowedIPs = 0.0.0.0/0, ::/0\nEndpoint = de.example.com:13231\nPersistentKeepalive = 25\n";

    [TestMethod]
    public void ApplyCustomDnsOverride_ReturnsConfigUnchanged_WhenNoCustomDnsConfigured()
    {
        string result = WireGuardConfigDnsPatcher.ApplyCustomDnsOverride(SERVER_ISSUED_CONFIG, new List<string>());

        result.Should().Be(SERVER_ISSUED_CONFIG);
    }

    [TestMethod]
    public void ApplyCustomDnsOverride_ReturnsConfigUnchanged_WhenCustomDnsIsNull()
    {
        string result = WireGuardConfigDnsPatcher.ApplyCustomDnsOverride(SERVER_ISSUED_CONFIG, null);

        result.Should().Be(SERVER_ISSUED_CONFIG);
    }

    [TestMethod]
    public void ApplyCustomDnsOverride_ReplacesDnsLineOnly_WhenCustomDnsConfigured()
    {
        string result = WireGuardConfigDnsPatcher.ApplyCustomDnsOverride(SERVER_ISSUED_CONFIG, new List<string> { "9.9.9.9" });

        result.Should().Contain("DNS = 9.9.9.9");
        result.Should().NotContain("1.1.1.1");
        result.Should().Contain("PrivateKey = CLIENT_PRIVATE_KEY");
        result.Should().Contain("Endpoint = de.example.com:13231");
    }

    [TestMethod]
    public void ApplyCustomDnsOverride_JoinsMultipleCustomDnsServers()
    {
        string result = WireGuardConfigDnsPatcher.ApplyCustomDnsOverride(SERVER_ISSUED_CONFIG, new List<string> { "9.9.9.9", "149.112.112.112" });

        result.Should().Contain("DNS = 9.9.9.9, 149.112.112.112");
    }
}
