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

using Newtonsoft.Json;

namespace SyncVPN.Api.V2.Contracts.Account;

// POST /account - claims (creates, retrieves, or changes) the current device's VPN account on a
// specific server. An empty request retrieves the existing active account for this Deviceid, if any.
// Transport is only valid for OpenVpn and must be omitted for WireGuard, L2tp, and Sstp, per the API
// contract.
public class ClaimAccountRequest
{
    [JsonProperty("server_id", NullValueHandling = NullValueHandling.Ignore)]
    public long? ServerId { get; set; }

    [JsonProperty("protocol", NullValueHandling = NullValueHandling.Ignore)]
    public string? Protocol { get; set; }

    [JsonProperty("transport", NullValueHandling = NullValueHandling.Ignore)]
    public string? Transport { get; set; }
}

// POST /account/city - same claim semantics as ClaimAccountRequest, but lets the backend auto-pick
// the least-loaded active server in the given city instead of naming a server_id directly.
public class ClaimAccountByCityRequest
{
    [JsonProperty("country_id")]
    public long CountryId { get; set; }

    [JsonProperty("city")]
    public string City { get; set; } = string.Empty;

    [JsonProperty("protocol", NullValueHandling = NullValueHandling.Ignore)]
    public string? Protocol { get; set; }

    [JsonProperty("transport", NullValueHandling = NullValueHandling.Ignore)]
    public string? Transport { get; set; }
}

// Protocol/transport string values, as required by ClaimAccountRequest/ClaimAccountByCityRequest.
public static class SyncVpnProtocols
{
    public const string WireGuard = "WireGuard";
    public const string OpenVpn = "OpenVpn";
    public const string L2tp = "L2tp";
    public const string Sstp = "Sstp";
}

public static class SyncVpnTransports
{
    public const string Tcp = "tcp";
    public const string Udp = "udp";
}
