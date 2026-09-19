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

namespace SyncVPN.Api.V2.Contracts.CheckoutLinks;

// POST /checkout-links - creates a one-hour, single-invoice browser checkout link. Email is required
// for a guest device (no logged-in account) and ignored server-side for a logged-in one (extracted
// from the account instead). Action=renew requires the device's own Bearer token (attached
// automatically by SyncVpnApiClient.CreateRequest) and is not available to guests.
public class CheckoutLinkRequest
{
    [JsonProperty("plan_id")]
    public long PlanId { get; set; }

    [JsonProperty("action")]
    public string Action { get; set; } = CheckoutLinkActions.Purchase;

    [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
    public string? Email { get; set; }

    [JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
    public string? Currency { get; set; }

    // Only meaningful with Action=Renew - optional, defaults server-side to the user's last completed
    // subscription when omitted. Not currently set by anything in this codebase (no renew UI yet).
    [JsonProperty("renewal_transaction_id", NullValueHandling = NullValueHandling.Ignore)]
    public long? RenewalTransactionId { get; set; }
}

public static class CheckoutLinkActions
{
    public const string Purchase = "purchase";
    public const string Renew = "renew";
}
