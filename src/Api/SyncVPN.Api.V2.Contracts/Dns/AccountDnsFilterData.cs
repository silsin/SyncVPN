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

namespace SyncVPN.Api.V2.Contracts.Dns;

public class AccountDnsFilterData
{
    [JsonProperty("account_name")]
    public string AccountName { get; set; } = string.Empty;

    [JsonProperty("subscription_active")]
    public bool SubscriptionActive { get; set; }

    // Only true when the account is Pro, active, unexpired, and owned by the current DeviceToken.
    [JsonProperty("can_update")]
    public bool CanUpdate { get; set; }

    [JsonProperty("filters")]
    public DnsFilterStates Filters { get; set; } = new();

    [JsonProperty("revision")]
    public long Revision { get; set; }

    [JsonProperty("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
