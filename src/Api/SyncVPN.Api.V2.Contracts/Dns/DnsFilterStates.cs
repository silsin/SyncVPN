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

// The full 8-category state, as returned by GET /account/dns-filters - every field always present,
// values are literally "on"/"off" per the API contract (not booleans).
public class DnsFilterStates
{
    [JsonProperty("malware")]
    public string Malware { get; set; } = "off";

    [JsonProperty("ads_trackers")]
    public string AdsTrackers { get; set; } = "off";

    [JsonProperty("social_networks")]
    public string SocialNetworks { get; set; } = "off";

    [JsonProperty("porn")]
    public string Porn { get; set; } = "off";

    [JsonProperty("gambling")]
    public string Gambling { get; set; } = "off";

    [JsonProperty("clickbait")]
    public string Clickbait { get; set; } = "off";

    [JsonProperty("other_vpns")]
    public string OtherVpns { get; set; } = "off";

    [JsonProperty("crypto")]
    public string Crypto { get; set; } = "off";
}
