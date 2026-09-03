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

namespace SyncVPN.Api.V2.Contracts.Servers;

// POST /servers/{server}/rate - integer 1-5. Resending updates this device's previous rating for
// the server rather than adding a second one.
public class RateServerRequest
{
    [JsonProperty("rate")]
    public int Rate { get; set; }
}

public class RateServerData
{
    [JsonProperty("server_id")]
    public long ServerId { get; set; }

    [JsonProperty("rate")]
    public int Rate { get; set; }

    // Average of this server's still-valid ratings (each rating expires after one month).
    [JsonProperty("server_rate")]
    public double ServerRate { get; set; }
}

// 200 when updating this device's existing rating, 201 when submitting a new one.
public class RateServerResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("data")]
    public RateServerData Data { get; set; } = new();
}
