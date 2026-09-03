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

// Present on PurchasedAccount only when protocol == OpenVpn. Configuration embeds this backend's own
// CA certificate - it must never be combined with the legacy Proton OpenVpn config template, which
// hardcodes Proton's CA and would fail the TLS handshake against this backend's servers.
public class OpenVpnConnectionInfo
{
    [JsonProperty("transport")]
    public string Transport { get; set; } = string.Empty;

    [JsonProperty("remote")]
    public string Remote { get; set; } = string.Empty;

    [JsonProperty("port")]
    public int Port { get; set; }

    [JsonProperty("available_ports")]
    public List<int> AvailablePorts { get; set; } = [];

    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;

    [JsonProperty("password")]
    public string Password { get; set; } = string.Empty;

    [JsonProperty("configuration")]
    public string Configuration { get; set; } = string.Empty;
}
