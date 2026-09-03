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

public class ClaimAccountResponseData
{
    [JsonProperty("account")]
    public PurchasedAccount Account { get; set; } = new();
}

public class ClaimAccountResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    // true means an existing account was returned as-is rather than (re)created.
    [JsonProperty("replayed")]
    public bool Replayed { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("data")]
    public ClaimAccountResponseData Data { get; set; } = new();
}
