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

using System;
using Newtonsoft.Json;

namespace SyncVPN.Api.V2.Contracts.Account;

public class AccountUsage
{
    [JsonProperty("sent_mb")]
    public double SentMb { get; set; }

    [JsonProperty("received_mb")]
    public double ReceivedMb { get; set; }
}

public class UsageAccount
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("usage")]
    public AccountUsage Usage { get; set; } = new();

    [JsonProperty("last_connected_ip")]
    public string? LastConnectedIp { get; set; }

    [JsonProperty("last_connected_at")]
    public DateTimeOffset? LastConnectedAt { get; set; }
}

public class UsageReportResponseData
{
    [JsonProperty("account")]
    public UsageAccount Account { get; set; } = new();
}

public class UsageReportResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    // true means this report_id was already applied and this call was a no-op replay, not a fresh count.
    [JsonProperty("replayed")]
    public bool Replayed { get; set; }

    [JsonProperty("data")]
    public UsageReportResponseData Data { get; set; } = new();
}
