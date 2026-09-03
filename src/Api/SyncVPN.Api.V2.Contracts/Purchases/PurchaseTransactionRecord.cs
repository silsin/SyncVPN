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

// The transaction record embedded in a PurchaseResponse. AmountMinor is the gross amount confirmed
// by the store; SubtotalMinor/TaxMinor are split out from AmountMinor using the billing country's
// tax-rate snapshot at purchase time (TaxRate), not the country's current rate.
public class PurchaseTransactionRecord
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("idempotency_key")]
    public Guid IdempotencyKey { get; set; }

    [JsonProperty("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("environment")]
    public string Environment { get; set; } = string.Empty;

    [JsonProperty("provider_transaction_id")]
    public string? ProviderTransactionId { get; set; }

    [JsonProperty("provider_order_id")]
    public string? ProviderOrderId { get; set; }

    [JsonProperty("product_id")]
    public string? ProductId { get; set; }

    [JsonProperty("country_id")]
    public long CountryId { get; set; }

    [JsonProperty("vat_tax")]
    public string? VatTax { get; set; }

    [JsonProperty("amount_minor")]
    public long AmountMinor { get; set; }

    [JsonProperty("subtotal_minor")]
    public long SubtotalMinor { get; set; }

    [JsonProperty("tax_rate")]
    public decimal TaxRate { get; set; }

    [JsonProperty("tax_minor")]
    public long TaxMinor { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("purchased_at")]
    public DateTimeOffset? PurchasedAt { get; set; }

    [JsonProperty("expires_at")]
    public DateTimeOffset? ExpiresAt { get; set; }

    [JsonProperty("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }
}
