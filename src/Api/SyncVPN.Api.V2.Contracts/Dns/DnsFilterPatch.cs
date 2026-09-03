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

namespace SyncVPN.Api.V2.Contracts.Dns;

// Partial update for PATCH /account/dns-filters - every field is optional (1 to 8 of them),
// null fields are omitted from the request entirely rather than sent as "off".
public class DnsFilterPatch
{
    [JsonProperty("malware", NullValueHandling = NullValueHandling.Ignore)]
    public string? Malware { get; set; }

    [JsonProperty("ads_trackers", NullValueHandling = NullValueHandling.Ignore)]
    public string? AdsTrackers { get; set; }

    [JsonProperty("social_networks", NullValueHandling = NullValueHandling.Ignore)]
    public string? SocialNetworks { get; set; }

    [JsonProperty("porn", NullValueHandling = NullValueHandling.Ignore)]
    public string? Porn { get; set; }

    [JsonProperty("gambling", NullValueHandling = NullValueHandling.Ignore)]
    public string? Gambling { get; set; }

    [JsonProperty("clickbait", NullValueHandling = NullValueHandling.Ignore)]
    public string? Clickbait { get; set; }

    [JsonProperty("other_vpns", NullValueHandling = NullValueHandling.Ignore)]
    public string? OtherVpns { get; set; }

    [JsonProperty("crypto", NullValueHandling = NullValueHandling.Ignore)]
    public string? Crypto { get; set; }
}
