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

namespace SyncVPN.Api.V2.Contracts.Transactions;

// GET /transactions?page=N - newest first, fixed page size of 10 (Meta.PerPage). A page beyond the
// last one comes back with an empty Data list rather than an error.
public class TransactionListMeta
{
    [JsonProperty("page")]
    public int Page { get; set; }

    [JsonProperty("per_page")]
    public int PerPage { get; set; }

    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("last_page")]
    public int LastPage { get; set; }

    [JsonProperty("has_more")]
    public bool HasMore { get; set; }

    [JsonProperty("next_page")]
    public int? NextPage { get; set; }

    [JsonProperty("previous_page")]
    public int? PreviousPage { get; set; }
}

public class TransactionListResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("data")]
    public List<TransactionListItem> Data { get; set; } = [];

    [JsonProperty("meta")]
    public TransactionListMeta Meta { get; set; } = new();
}
