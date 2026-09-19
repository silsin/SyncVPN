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
using SyncVPN.Crypto.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Crypto;
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
                case VpnProtocol.L2tp:
                    return (SyncVpnProtocols.L2tp, null);
                case VpnProtocol.Sstp:
                    return (SyncVpnProtocols.Sstp, null);
            }
        }

        return (SyncVpnProtocols.OpenVpn, SyncVpnTransports.Tcp);
    }

    // Given a specific server's advertised protocols, picks what it will actually be claimed with:
    // the user's global preference if that server supports it, otherwise the same
    // WireGuard-if-present-else-first fallback used when claiming without a specific server in mind.
    // Unlike ResolveClaimProtocol, this never fails to return something usable - every server has at
    // least one protocol - and it's honest about the result rather than a preference the caller must
    // trust blindly, so UI can show the caller what will actually happen.
    public static string ResolveServerProtocol(VpnProtocol preferredProtocol, IReadOnlyList<string> serverProtocols)
    {
        string? preferredWireProtocol = MapToWireProtocol(preferredProtocol);
        if (preferredWireProtocol is not null && serverProtocols.Contains(preferredWireProtocol))
        {
            return preferredWireProtocol;
        }

        return serverProtocols.Contains(SyncVpnProtocols.WireGuard)
            ? SyncVpnProtocols.WireGuard
            : serverProtocols.FirstOrDefault() ?? SyncVpnProtocols.WireGuard;
    }

    // Same protocol pick as ResolveServerProtocol, plus the Transport (udp/tcp) that POST /account
    // requires whenever the resolved protocol is OpenVpn - the backend rejects an OpenVpn claim with no
    // transport at all ("The transport field is required"), unlike WireGuard/L2tp/Sstp which have none.
    // Used by every call site that connects straight to one specific server (a map pin or a free-server
    // list row) rather than through ResolveClaimProtocol's candidate-list claim.
    public static (string Protocol, string? Transport) ResolveServerProtocolAndTransport(VpnProtocol preferredProtocol, IReadOnlyList<string> serverProtocols)
    {
        string protocol = ResolveServerProtocol(preferredProtocol, serverProtocols);

        if (protocol != SyncVpnProtocols.OpenVpn)
        {
            return (protocol, null);
        }

        // preferredProtocol names the transport directly when it's what actually got resolved to
        // OpenVpn; for the WireGuard-else-first fallback path (e.g. preferredProtocol is Smart, or this
        // server doesn't support the user's preferred transport), default to udp - the more commonly
        // supported OpenVpn transport.
        string transport = preferredProtocol == VpnProtocol.OpenVpnTcp ? SyncVpnTransports.Tcp : SyncVpnTransports.Udp;
        return (protocol, transport);
    }

    // Smart has no single wire protocol (it's a client-side auto-selection mode), and WireGuardTcp/
    // WireGuardTls have no equivalent on this backend at all (see ResolveClaimProtocol) - null for all
    // three. Also used by Settings/profile protocol pickers to decide whether a given VpnProtocol
    // value corresponds to something the backend can actually offer at all.
    public static string? MapToWireProtocol(VpnProtocol protocol)
    {
        return protocol switch
        {
            VpnProtocol.WireGuardUdp => SyncVpnProtocols.WireGuard,
            VpnProtocol.OpenVpnUdp or VpnProtocol.OpenVpnTcp => SyncVpnProtocols.OpenVpn,
            VpnProtocol.L2tp => SyncVpnProtocols.L2tp,
            VpnProtocol.Sstp => SyncVpnProtocols.Sstp,
            _ => null,
        };
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
                // Without this, VpnEndpointScanner.EndpointCandidates skips WireGuardUdp/OpenVpnUdp
                // entirely for this server (it treats a null key as "can't ping this protocol") and
                // falls back to TCP port probes the server never actually listens on - the endpoint
                // scan then always fails with PingTimeoutError before a real connection is ever
                // attempted, regardless of network conditions.
                X25519PublicKey = string.IsNullOrEmpty(account.ServerPublicKey)
                    ? null
                    : new ServerPublicKeyIpcEntity(new PublicKey(account.ServerPublicKey, KeyAlgorithm.X25519)),
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
        switch (account.Protocol)
        {
            case SyncVpnProtocols.WireGuard:
                return BuildProvisionedConfigCredentials(account.WireGuard?.Configuration, account.Username, account.Password);
            case SyncVpnProtocols.OpenVpn:
                return BuildProvisionedConfigCredentials(account.OpenVpn?.Configuration, account.OpenVpn?.Username ?? account.Username,
                    account.OpenVpn?.Password ?? account.Password);
            case SyncVpnProtocols.L2tp:
                if (account.L2tp is null)
                {
                    throw new InvalidOperationException("Claimed SyncVPN account has no L2TP connection details.");
                }

                return new VpnCredentialsIpcEntity
                {
                    Certificate = null,
                    ClientKeyPair = null,
                    Username = account.L2tp.Username,
                    Password = account.L2tp.Password,
                    ProvisionedConfigText = null,
                    PreSharedKey = account.L2tp.Secret,
                };
            case SyncVpnProtocols.Sstp:
                if (account.Sstp is null)
                {
                    throw new InvalidOperationException("Claimed SyncVPN account has no SSTP connection details.");
                }

                return new VpnCredentialsIpcEntity
                {
                    Certificate = null,
                    ClientKeyPair = null,
                    Username = account.Sstp.Username,
                    Password = account.Sstp.Password,
                    ProvisionedConfigText = null,
                    PreSharedKey = null,
                };
            default:
                throw new InvalidOperationException($"Claimed SyncVPN account has an unsupported protocol '{account.Protocol}'.");
        }
    }

    private static VpnCredentialsIpcEntity BuildProvisionedConfigCredentials(string? configText, string username, string password)
    {
        if (string.IsNullOrEmpty(configText))
        {
            throw new InvalidOperationException("Claimed SyncVPN account has no usable connection config.");
        }

        return new VpnCredentialsIpcEntity
        {
            Certificate = null,
            ClientKeyPair = null,
            Username = username,
            Password = password,
            ProvisionedConfigText = configText,
        };
    }
}
