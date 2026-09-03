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

namespace SyncVPN.Api.V2.Contracts.Auth;

public class User
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;

    [JsonProperty("phone")]
    public string? Phone { get; set; }

    [JsonProperty("address")]
    public string? Address { get; set; }

    // Non-secret UUID - to be used as the RevenueCat/StoreKit app-user id (Phase 5/billing scope, not this phase).
    [JsonProperty("purchase_account_token")]
    public string PurchaseAccountToken { get; set; } = string.Empty;

    [JsonProperty("google_obfuscated_account_id")]
    public string GoogleObfuscatedAccountId { get; set; } = string.Empty;
}
