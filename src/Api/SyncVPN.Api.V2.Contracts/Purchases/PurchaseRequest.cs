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

namespace SyncVPN.Api.V2.Contracts.Purchases;

// POST /purchases - records and verifies a store/PSP receipt for a guest or a logged-in user, and
// provisions Pro access. For a guest purchase, the app must set IdempotencyKey to a UUID that is
// also used as the RevenueCat App User ID, and keep it fixed across retries of the same purchase.
// A retry with the same IdempotencyKey and the same billing fields replays the original purchase and
// login_code rather than creating a new one - see PurchaseResponse.Replayed.
public class PurchaseRequest
{
    [JsonProperty("idempotency_key")]
    public Guid IdempotencyKey { get; set; }

    [JsonProperty("plan_id")]
    public long PlanId { get; set; }

    // Billing country from GET /billing/countries - required.
    [JsonProperty("country_id")]
    public long CountryId { get; set; }

    // Optional customer VAT/tax id, stored with the transaction.
    [JsonProperty("vat_tax", NullValueHandling = NullValueHandling.Ignore)]
    public string? VatTax { get; set; }

    [JsonProperty("provider")]
    public string Provider { get; set; } = string.Empty;

    // Provider-specific receipt payload. Only the "google_play" shape (GooglePlayPurchaseTransaction)
    // is documented so far - left as object so this contract doesn't assert a schema it doesn't have
    // for "apple_app_store"/"stripe". TODO: add a StripePurchaseTransaction type once that provider's
    // request shape is specified; nothing in this codebase should call SubmitPurchaseAsync with
    // provider "stripe" until then.
    [JsonProperty("transaction")]
    public object Transaction { get; set; } = new();
}

public static class SyncVpnPurchaseProviders
{
    public const string GooglePlay = "google_play";
    public const string AppleAppStore = "apple_app_store";
    public const string Stripe = "stripe";
}

// The only documented provider-specific transaction payload.
public class GooglePlayPurchaseTransaction
{
    [JsonProperty("product_id")]
    public string ProductId { get; set; } = string.Empty;
}
