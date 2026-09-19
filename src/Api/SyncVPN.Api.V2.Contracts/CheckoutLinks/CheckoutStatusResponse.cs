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
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SyncVPN.Api.V2.Contracts.CheckoutLinks;

public class CheckoutStatusResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    // True when this checkout link was started as a guest (no logged-in account) - Data.Authentication
    // is how the app actually signs that guest in once State is Completed, not this flag alone.
    [JsonProperty("requires_login")]
    public bool RequiresLogin { get; set; }

    [JsonProperty("data")]
    public CheckoutStatusData? Data { get; set; }
}

public class CheckoutStatusData
{
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    // One of CheckoutLinkStates. pending/confirming mean "keep polling"; the other four are terminal.
    [JsonProperty("state")]
    public string State { get; set; } = string.Empty;

    [JsonProperty("payment_expires_at")]
    public DateTimeOffset? PaymentExpiresAt { get; set; }

    // True while a crypto payment's blockchain confirmation can't currently be checked - transient, not
    // a failure; the state stays "confirming" (or "pending") until this clears, so callers should just
    // keep polling rather than treating it as an error on its own.
    [JsonProperty("verification_pending")]
    public bool VerificationPending { get; set; }

    // Only populated for a completed guest purchase, for up to 24h - null otherwise (including for a
    // logged-in device's own purchase, which needs no credential exchange).
    [JsonProperty("authentication")]
    public CheckoutStatusAuthentication? Authentication { get; set; }

    [JsonProperty("plan")]
    public CheckoutStatusPlan? Plan { get; set; }

    // Populated once a payment attempt exists (financial details, gateway id, payer, currency,
    // subscription end date) - shape isn't documented yet, so left untyped like PurchaseRequest.Transaction.
    [JsonProperty("transaction")]
    public object? Transaction { get; set; }
}

// Exactly one of LoginCode or (Email, Password) is populated, never both - exchange via
// IUserAuthenticator.LoginWithCodeAsync or LoginUserAsync respectively.
public class CheckoutStatusAuthentication
{
    [JsonProperty("login_code")]
    public string? LoginCode { get; set; }

    [JsonProperty("email")]
    public string? Email { get; set; }

    // A temporary 5-character password paired with Email.
    [JsonProperty("password")]
    public string? Password { get; set; }
}

// Smaller subset of Plans.Plan returned alongside checkout status - no prices/products, since the
// price was already fixed when the checkout link was created.
public class CheckoutStatusPlan
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("months")]
    public int Months { get; set; }

    [JsonProperty("users")]
    public int Users { get; set; }

    [JsonProperty("protocols")]
    public List<string> Protocols { get; set; } = [];
}

public static class CheckoutLinkStates
{
    public const string Pending = "pending";
    public const string Confirming = "confirming";
    public const string Completed = "completed";
    public const string Expired = "expired";
    public const string Failed = "failed";
    public const string Refunded = "refunded";
}
