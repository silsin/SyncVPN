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

using SyncVPN.Api.V2.Contracts.Account;
using SyncVPN.Client.Logic.Connection.RequestCreators;
using SyncVPN.Common.Core.Networking;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace SyncVPN.Client.Logic.Connection.Tests.RequestCreators;

[TestClass]
public class SyncVpnAccountClaimMapperTest
{
    [TestMethod]
    public void ResolveClaimProtocol_ReturnsWireGuard_WhenWireGuardUdpIsPreferred()
    {
        (string protocol, string? transport) = SyncVpnAccountClaimMapper.ResolveClaimProtocol(
            [VpnProtocol.WireGuardUdp, VpnProtocol.OpenVpnTcp]);

        Assert.AreEqual(SyncVpnProtocols.WireGuard, protocol);
        Assert.IsNull(transport);
    }

    [TestMethod]
    public void ResolveClaimProtocol_ReturnsOpenVpnUdp_WhenOpenVpnUdpIsPreferred()
    {
        (string protocol, string? transport) = SyncVpnAccountClaimMapper.ResolveClaimProtocol(
            [VpnProtocol.OpenVpnUdp, VpnProtocol.OpenVpnTcp]);

        Assert.AreEqual(SyncVpnProtocols.OpenVpn, protocol);
        Assert.AreEqual(SyncVpnTransports.Udp, transport);
    }

    [TestMethod]
    public void ResolveClaimProtocol_SkipsUnsupportedProtocols_AndFallsBackToNextPreference()
    {
        // WireGuardTcp/WireGuardTls have no equivalent on the new backend.
        (string protocol, string? transport) = SyncVpnAccountClaimMapper.ResolveClaimProtocol(
            [VpnProtocol.WireGuardTcp, VpnProtocol.WireGuardTls, VpnProtocol.OpenVpnUdp]);

        Assert.AreEqual(SyncVpnProtocols.OpenVpn, protocol);
        Assert.AreEqual(SyncVpnTransports.Udp, transport);
    }

    [TestMethod]
    public void ResolveClaimProtocol_FallsBackToOpenVpnTcp_WhenNothingPreferredIsSupported()
    {
        (string protocol, string? transport) = SyncVpnAccountClaimMapper.ResolveClaimProtocol(
            [VpnProtocol.WireGuardTcp, VpnProtocol.WireGuardTls]);

        Assert.AreEqual(SyncVpnProtocols.OpenVpn, protocol);
        Assert.AreEqual(SyncVpnTransports.Tcp, transport);
    }

    [TestMethod]
    public void BuildClaimedServer_UsesHostnameAndSkipsSignatureValidation()
    {
        PurchasedAccount account = new() { ServerHostname = "de.example.com", ServerIp = "198.51.100.30" };

        VpnServerIpcEntity[] servers = SyncVpnAccountClaimMapper.BuildClaimedServer(account);

        Assert.AreEqual(1, servers.Length);
        Assert.AreEqual("de.example.com", servers[0].Name);
        Assert.AreEqual("198.51.100.30", servers[0].Ip);
        Assert.IsTrue(servers[0].SkipSignatureValidation);
        Assert.IsNull(servers[0].X25519PublicKey);
    }

    [TestMethod]
    public void BuildClaimedServer_FallsBackToServerIp_WhenHostnameIsMissing()
    {
        PurchasedAccount account = new() { ServerHostname = null, ServerIp = "198.51.100.30" };

        VpnServerIpcEntity[] servers = SyncVpnAccountClaimMapper.BuildClaimedServer(account);

        Assert.AreEqual("198.51.100.30", servers[0].Name);
    }

    [TestMethod]
    public void BuildClaimedServer_Throws_WhenNoHostnameOrIpAvailable()
    {
        PurchasedAccount account = new() { ServerHostname = null, ServerIp = null };

        Assert.ThrowsExactly<InvalidOperationException>(() => SyncVpnAccountClaimMapper.BuildClaimedServer(account));
    }

    [TestMethod]
    public void BuildClaimedCredentials_UsesWireGuardConfiguration_ForWireGuardAccount()
    {
        PurchasedAccount account = new()
        {
            Protocol = SyncVpnProtocols.WireGuard,
            Username = "ab12cd34",
            Password = "4721",
            WireGuard = new WireGuardConnectionInfo { Configuration = "[Interface]\nPrivateKey = SERVER_ISSUED" },
        };

        VpnCredentialsIpcEntity credentials = SyncVpnAccountClaimMapper.BuildClaimedCredentials(account);

        Assert.AreEqual("[Interface]\nPrivateKey = SERVER_ISSUED", credentials.ProvisionedConfigText);
        Assert.IsNull(credentials.ClientKeyPair);
        Assert.IsNull(credentials.Certificate);
        Assert.AreEqual("ab12cd34", credentials.Username);
        Assert.AreEqual("4721", credentials.Password);
    }

    [TestMethod]
    public void BuildClaimedCredentials_UsesOpenVpnConfigurationAndCredentials_ForOpenVpnAccount()
    {
        PurchasedAccount account = new()
        {
            Protocol = SyncVpnProtocols.OpenVpn,
            Username = "account-username",
            Password = "account-password",
            OpenVpn = new OpenVpnConnectionInfo
            {
                Configuration = "client\ndev tun",
                Username = "ef34gh56",
                Password = "0831",
            },
        };

        VpnCredentialsIpcEntity credentials = SyncVpnAccountClaimMapper.BuildClaimedCredentials(account);

        Assert.AreEqual("client\ndev tun", credentials.ProvisionedConfigText);
        Assert.AreEqual("ef34gh56", credentials.Username);
        Assert.AreEqual("0831", credentials.Password);
    }

    [TestMethod]
    public void BuildClaimedCredentials_Throws_WhenConfigurationIsMissing()
    {
        PurchasedAccount account = new() { Protocol = SyncVpnProtocols.WireGuard, WireGuard = null };

        Assert.ThrowsExactly<InvalidOperationException>(() => SyncVpnAccountClaimMapper.BuildClaimedCredentials(account));
    }
}
