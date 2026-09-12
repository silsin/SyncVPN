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

using System;
using Newtonsoft.Json;
using SyncVPN.Api.V2.Contracts.Dns;

namespace SyncVPN.Api.V2.Contracts.Account;

// The account/credentials returned by POST /account or POST /account/city - this is the only place
// connection secrets (WireGuard keys, OpenVpn credentials, ready-to-import config text) appear. The
// public server catalog (GET /servers) never carries them - see ServerListItem's remarks.
public class PurchasedAccount
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;

    [JsonProperty("password")]
    public string Password { get; set; } = string.Empty;

    [JsonProperty("protocol")]
    public string Protocol { get; set; } = string.Empty;

    [JsonProperty("transport")]
    public string? Transport { get; set; }

    // Full config text for whichever protocol was selected, ready for import - same content as
    // WireGuard.Configuration / OpenVpn.Configuration below, kept here for convenience.
    [JsonProperty("config")]
    public string Config { get; set; } = string.Empty;

    [JsonProperty("free")]
    public bool Free { get; set; }

    [JsonProperty("plan_id")]
    public long? PlanId { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; } = string.Empty;

    [JsonProperty("service_id")]
    public long ServiceId { get; set; }

    [JsonProperty("server_id")]
    public long ServerId { get; set; }

    [JsonProperty("server_hostname")]
    public string? ServerHostname { get; set; }

    [JsonProperty("server_ip")]
    public string? ServerIp { get; set; }

    [JsonProperty("ip")]
    public string? Ip { get; set; }

    [JsonProperty("private_key")]
    public string? PrivateKey { get; set; }

    [JsonProperty("public_key")]
    public string? PublicKey { get; set; }

    [JsonProperty("server_public_key")]
    public string? ServerPublicKey { get; set; }

    [JsonProperty("wireguard")]
    public WireGuardConnectionInfo? WireGuard { get; set; }

    [JsonProperty("openvpn")]
    public OpenVpnConnectionInfo? OpenVpn { get; set; }

    [JsonProperty("l2tp")]
    public L2tpConnectionInfo? L2tp { get; set; }

    [JsonProperty("sstp")]
    public SstpConnectionInfo? Sstp { get; set; }

    [JsonProperty("expires_at")]
    public DateTimeOffset? ExpiresAt { get; set; }

    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("dns_filters")]
    public DnsFilterStates DnsFilters { get; set; } = new();

    [JsonProperty("dns_filters_can_update")]
    public bool DnsFiltersCanUpdate { get; set; }
}
