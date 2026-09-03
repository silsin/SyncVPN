/*
 * Copyright (c) 2025 Proton AG
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

using System;
using SyncVPN.Common.Legacy.Helpers;
using SyncVPN.Crypto.Contracts;

namespace SyncVPN.Common.Legacy.Vpn;

public readonly struct VpnCredentials
{
    public VpnCredentials(
        string clientCertPem,
        DateTime? clientCertificateExpirationDateUtc,
        AsymmetricKeyPair clientKeyPair,
        string username,
        string password,
        string provisionedConfigText = null)
    {
        // A server-issued config (new SyncVPN backend) carries no client keypair/certificate at all -
        // only require one when there's no ready-made config to fall back on.
        if (provisionedConfigText is null)
        {
            Ensure.NotNull(clientKeyPair, nameof(clientKeyPair));
        }

        ClientCertPem = clientCertPem;
        ClientCertificateExpirationDateUtc = clientCertificateExpirationDateUtc;
        ClientKeyPair = clientKeyPair;
        Username = username;
        Password = password;
        ProvisionedConfigText = provisionedConfigText;
    }

    public VpnCredentials(AsymmetricKeyPair clientKeyPair) : this(string.Empty, null, clientKeyPair, string.Empty, string.Empty)
    {
    }

    public VpnCredentials(AsymmetricKeyPair clientKeyPair, string username, string password) : this(string.Empty, null, clientKeyPair, username, password)
    {
    }

    // Full ready-to-write WireGuard or OpenVpn config text, as returned by the new SyncVPN backend's
    // POST /account. When set, WireGuardConnection/OpenVpnConnection write it verbatim instead of
    // building a config from ClientKeyPair/ClientCertPem - see the migration plan's Phase 2.
    public static VpnCredentials FromProvisionedConfig(string provisionedConfigText, string username, string password)
    {
        return new VpnCredentials(string.Empty, null, null, username, password, provisionedConfigText);
    }

    public string Username { get; }
    public string Password { get; }

    public string ClientCertPem { get; }
    public DateTime? ClientCertificateExpirationDateUtc { get; }
    public AsymmetricKeyPair ClientKeyPair { get; }
    public string ProvisionedConfigText { get; }
}