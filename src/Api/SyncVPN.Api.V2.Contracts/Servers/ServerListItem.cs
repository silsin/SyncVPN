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

namespace SyncVPN.Api.V2.Contracts.Servers;

// This is the public server catalog (GET /servers) - it has no connection secrets (no WireGuard/OpenVpn
// key material, no entry IP). Those only come back from POST /account once a server+protocol is claimed
// for this device - see the migration plan's Phase 5 note on the new account-provisioning model.
public class ServerListItem
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("country")]
    public Country Country { get; set; } = new();

    [JsonProperty("city")]
    public string? City { get; set; }

    [JsonProperty("datacenter")]
    public string? Datacenter { get; set; }

    [JsonProperty("location")]
    public GeoLocation? Location { get; set; }

    [JsonProperty("protocols")]
    public List<string> Protocols { get; set; } = [];

    // 1 = free server, 0 = Pro-only.
    [JsonProperty("free")]
    public int Free { get; set; }

    [JsonProperty("rate")]
    public float Rate { get; set; }
}
