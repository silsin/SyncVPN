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

// POST /account/usage - reports incremental (delta) usage since the last report; the backend adds these
// deltas onto the account's running totals rather than replacing them. report_id must be a fresh UUID
// per interval: retrying the same id with the same payload is a no-op (replayed: true in the response),
// but reusing it with a different payload is a 409. connected_ip/connected_at are optional and only
// meaningful together - send them on the first report of a new connection, omit on later ones.
public class UsageReportRequest
{
    [JsonProperty("report_id")]
    public Guid ReportId { get; set; }

    [JsonProperty("sent_mb_delta")]
    public double SentMbDelta { get; set; }

    [JsonProperty("received_mb_delta")]
    public double ReceivedMbDelta { get; set; }

    [JsonProperty("connected_ip", NullValueHandling = NullValueHandling.Ignore)]
    public string? ConnectedIp { get; set; }

    [JsonProperty("connected_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? ConnectedAt { get; set; }
}
