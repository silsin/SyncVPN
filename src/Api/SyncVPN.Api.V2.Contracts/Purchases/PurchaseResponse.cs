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
using SyncVPN.Api.V2.Contracts.Account;

namespace SyncVPN.Api.V2.Contracts.Purchases;

public class PurchaseResponseData
{
    [JsonProperty("transaction")]
    public PurchaseTransactionRecord Transaction { get; set; } = new();

    // 16-character code the app must exchange via POST /auth/code-login (ISyncVpnAuthenticator.
    // LoginWithCodeAsync) to obtain a device token and reach Pro access.
    [JsonProperty("login_code")]
    public string? LoginCode { get; set; }

    [JsonProperty("has_active_purchase")]
    public bool HasActivePurchase { get; set; }

    // False right after a fresh (non-replayed) purchase confirmation - the caller must still pick a
    // Pro server (GET /servers/pro) and call POST /account to provision the VPN account. True (with
    // Account populated) when replaying an already-provisioned purchase.
    [JsonProperty("account_provisioned")]
    public bool AccountProvisioned { get; set; }

    [JsonProperty("account")]
    public PurchasedAccount? Account { get; set; }
}

// 200 = this exact purchase (same idempotency_key + billing details) was already completed and is
// being replayed as-is. 201 = payment verified and Pro access granted for the first time.
public class PurchaseResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("requires_login")]
    public bool RequiresLogin { get; set; }

    [JsonProperty("replayed")]
    public bool Replayed { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("data")]
    public PurchaseResponseData Data { get; set; } = new();
}
