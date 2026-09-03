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

namespace SyncVPN.Api.V2.Contracts.Plans;

// GET /plans - a currently purchasable plan. Protocols lists which of Servers.SyncVpnProtocols this
// plan permits on POST /account/{...} (a combined plan allows both). Prices/Products are keyed by
// currency code / store name respectively - Products' "google_play"/"apple_app_store" values are the
// store product ids for mobile; this Windows client has no equivalent entry and buys via provider
// "stripe" on POST /purchases instead (see PurchaseRequest).
public class Plan
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("service")]
    public string Service { get; set; } = string.Empty;

    [JsonProperty("protocols")]
    public List<string> Protocols { get; set; } = [];

    [JsonProperty("months")]
    public int Months { get; set; }

    [JsonProperty("users")]
    public int Users { get; set; }

    // Currency code (e.g. "eur", "usd") -> decimal-string price.
    [JsonProperty("prices")]
    public Dictionary<string, string> Prices { get; set; } = new();

    // Store name (e.g. "google_play", "apple_app_store") -> store product id.
    [JsonProperty("products")]
    public Dictionary<string, string> Products { get; set; } = new();
}

public class PlanListResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("data")]
    public List<Plan> Data { get; set; } = [];
}
