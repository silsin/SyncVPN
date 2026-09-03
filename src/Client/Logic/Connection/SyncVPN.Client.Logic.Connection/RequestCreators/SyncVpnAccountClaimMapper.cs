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
using SyncVPN.Common.Core.Networking;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace SyncVPN.Client.Logic.Connection.RequestCreators;

// Pure translation between the new SyncVPN backend's POST /account response and the IPC entities
// ConnectionRequestCreator sends to the Windows service - split out from ConnectionRequestCreator so
// this logic (protocol selection, claim-response mapping) is unit-testable without the request
// creator's many other dependencies. See the migration plan's VpnProvisioning phase.
public static class SyncVpnAccountClaimMapper
{
    // WireGuardTcp/WireGuardTls have no equivalent on the new backend (only WireGuard-over-UDP and
    // OpenVpn tcp/udp are offered) - skip past them to the next preferred protocol rather than failing.
    public static (string Protocol, string? Transport) ResolveClaimProtocol(IList<VpnProtocol> preferredProtocols)
    {
        foreach (VpnProtocol protocol in preferredProtocols)
        {
            switch (protocol)
            {
                case VpnProtocol.WireGuardUdp:
                    return (SyncVpnProtocols.WireGuard, null);
                case VpnProtocol.OpenVpnUdp:
                    return (SyncVpnProtocols.OpenVpn, SyncVpnTransports.Udp);
                case VpnProtocol.OpenVpnTcp:
                    return (SyncVpnProtocols.OpenVpn, SyncVpnTransports.Tcp);
            }
        }

        return (SyncVpnProtocols.OpenVpn, SyncVpnTransports.Tcp);
    }

    public static VpnServerIpcEntity[] BuildClaimedServer(PurchasedAccount account)
    {
        string? host = account.ServerHostname ?? account.ServerIp;
        if (string.IsNullOrEmpty(host))
        {
            throw new InvalidOperationException("Claimed SyncVPN account has no server hostname or IP.");
        }

        return
        [
            new VpnServerIpcEntity
            {
                Name = host,
                Ip = account.ServerIp ?? string.Empty,
                Label = string.Empty,
                X25519PublicKey = null,
                Signature = string.Empty,
                IsIpv6Supported = false,
                RelayIpByProtocol = null,
                // No signature to verify: this host came back on the same authenticated HTTPS response
                // that carried the credentials themselves, unlike Proton's separately-fetched server
                // list - see VpnHost.SkipSignatureValidation.
                SkipSignatureValidation = true,
            }
        ];
    }

    public static VpnCredentialsIpcEntity BuildClaimedCredentials(PurchasedAccount account)
    {
        string? configText = account.Protocol == SyncVpnProtocols.WireGuard
            ? account.WireGuard?.Configuration
            : account.OpenVpn?.Configuration;

        if (string.IsNullOrEmpty(configText))
        {
            throw new InvalidOperationException("Claimed SyncVPN account has no usable connection config.");
        }

        return new VpnCredentialsIpcEntity
        {
            Certificate = null,
            ClientKeyPair = null,
            Username = account.OpenVpn?.Username ?? account.Username,
            Password = account.OpenVpn?.Password ?? account.Password,
            ProvisionedConfigText = configText,
        };
    }
}
