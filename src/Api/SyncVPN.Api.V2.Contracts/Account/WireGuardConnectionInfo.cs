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
using Newtonsoft.Json;

namespace SyncVPN.Api.V2.Contracts.Account;

// Present on PurchasedAccount only when protocol == WireGuard. Unlike the legacy backend flow, the
// private key is generated server-side and handed back here - the client never generates its own
// WireGuard keypair for this backend. See the migration plan's Phase 2 note on why Configuration is
// written verbatim instead of being rebuilt client-side.
public class WireGuardConnectionInfo
{
    [JsonProperty("interface")]
    public WireGuardInterfaceInfo Interface { get; set; } = new();

    [JsonProperty("peer")]
    public WireGuardPeerInfo Peer { get; set; } = new();

    [JsonProperty("configuration")]
    public string Configuration { get; set; } = string.Empty;
}

public class WireGuardInterfaceInfo
{
    [JsonProperty("private_key")]
    public string PrivateKey { get; set; } = string.Empty;

    [JsonProperty("public_key")]
    public string PublicKey { get; set; } = string.Empty;

    [JsonProperty("address")]
    public string Address { get; set; } = string.Empty;

    [JsonProperty("dns")]
    public List<string> Dns { get; set; } = [];

    [JsonProperty("mtu")]
    public int Mtu { get; set; }
}

public class WireGuardPeerInfo
{
    [JsonProperty("public_key")]
    public string PublicKey { get; set; } = string.Empty;

    [JsonProperty("endpoint")]
    public string Endpoint { get; set; } = string.Empty;

    [JsonProperty("port")]
    public int Port { get; set; }

    [JsonProperty("available_ports")]
    public List<int> AvailablePorts { get; set; } = [];

    [JsonProperty("allowed_ips")]
    public List<string> AllowedIps { get; set; } = [];

    [JsonProperty("persistent_keepalive")]
    public int PersistentKeepalive { get; set; }
}
