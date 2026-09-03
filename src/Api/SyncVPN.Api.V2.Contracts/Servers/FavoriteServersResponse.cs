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

// Favorites are per-device (keyed by Deviceid, not by user), capped at Meta.Limit (10). A server
// dropping out of rotation is silently removed from every device's favorites.
public class FavoriteServersMeta
{
    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("limit")]
    public int Limit { get; set; }
}

// GET /servers/favorites
public class FavoriteServersResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("data")]
    public List<ServerListItem> Data { get; set; } = [];

    [JsonProperty("meta")]
    public FavoriteServersMeta Meta { get; set; } = new();
}

// POST /servers/{server}/favorite - 200 if it was already a favorite (unchanged), 201 if newly added.
public class FavoriteServerActionResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("data")]
    public ServerListItem Data { get; set; } = new();

    [JsonProperty("meta")]
    public FavoriteServersMeta Meta { get; set; } = new();
}

// DELETE /servers/{server}/favorite
public class RemoveFavoriteServerData
{
    [JsonProperty("server_id")]
    public long ServerId { get; set; }

    // false if the server was not a favorite to begin with - not an error.
    [JsonProperty("removed")]
    public bool Removed { get; set; }
}

public class RemoveFavoriteServerResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("data")]
    public RemoveFavoriteServerData Data { get; set; } = new();

    [JsonProperty("meta")]
    public FavoriteServersMeta Meta { get; set; } = new();
}
