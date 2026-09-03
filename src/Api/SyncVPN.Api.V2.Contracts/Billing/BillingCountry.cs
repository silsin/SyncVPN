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

namespace SyncVPN.Api.V2.Contracts.Billing;

// GET /billing/countries - purchase-eligible countries, independent of language/server presence.
// Purchasing is enabled by default for every country except Iran (ir), Syria (sy), Cuba (cu) and
// Iraq (iq). VatRate is the standard, admin-editable VAT/tax percentage; 0 and Taxable=false for
// countries with no tax.
public class BillingCountry
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("flag")]
    public string? Flag { get; set; }

    [JsonProperty("emoji")]
    public string? Emoji { get; set; }

    [JsonProperty("short_name")]
    public string ShortName { get; set; } = string.Empty;

    [JsonProperty("vat_rate")]
    public decimal VatRate { get; set; }

    [JsonProperty("taxable")]
    public bool Taxable { get; set; }
}
