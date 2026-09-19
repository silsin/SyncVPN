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

namespace SyncVPN.Api.V2.Contracts.CheckoutLinks;

public class CheckoutLinkResponseData
{
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    // Confidential polling secret for a not-yet-documented status-polling endpoint. Never log this,
    // send it to the browser, or put it in the payment URL - only PaymentUrl is meant to leave the app.
    [JsonProperty("poll_token")]
    public string PollToken { get; set; } = string.Empty;

    [JsonProperty("payment_url")]
    public string PaymentUrl { get; set; } = string.Empty;

    [JsonProperty("state")]
    public string State { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;

    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }

    [JsonProperty("poll_interval")]
    public int PollIntervalSeconds { get; set; }
}

public class CheckoutLinkResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("data")]
    public CheckoutLinkResponseData Data { get; set; } = new();
}
