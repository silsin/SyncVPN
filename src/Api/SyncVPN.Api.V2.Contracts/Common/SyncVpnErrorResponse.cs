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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace SyncVPN.Api.V2.Contracts.Common;

// Shared shape of SyncVpnApiClient failure bodies (401/403/404/409/422/429/503 across /account,
// /purchases, /servers/*, etc.) - not every endpoint returns every field. Parse this instead of
// string-matching ApiResponseResult<T>.Error so callers can branch on error_code.
public class SyncVpnErrorResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("error_code")]
    public string? ErrorCode { get; set; }

    [JsonProperty("retryable")]
    public bool Retryable { get; set; }

    [JsonProperty("message")]
    public string? Message { get; set; }

    [JsonProperty("requires_login")]
    public bool RequiresLogin { get; set; }

    [JsonProperty("requires_device_registration")]
    public bool RequiresDeviceRegistration { get; set; }

    [JsonProperty("requires_email")]
    public bool RequiresEmail { get; set; }

    [JsonProperty("requires_language")]
    public bool RequiresLanguage { get; set; }

    [JsonProperty("force_logout")]
    public bool ForceLogout { get; set; }

    [JsonProperty("blocked")]
    public bool Blocked { get; set; }

    [JsonProperty("blocked_scope")]
    public string? BlockedScope { get; set; }

    [JsonProperty("errors")]
    public Dictionary<string, List<string>>? Errors { get; set; }

    // Returns null for a body that isn't valid JSON (e.g. the HTML 404 page for an invalid AppToken,
    // or an empty body for a wrong HTTP method) rather than throwing - callers should treat null as
    // "no structured error available" and fall back to the raw ApiResponseResult<T>.Error string.
    public static SyncVpnErrorResponse? TryParse(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            return JsonConvert.DeserializeObject<SyncVpnErrorResponse>(body);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

// error_code values documented across the new SyncVPN backend endpoints. Not exhaustive of every
// possible future value - always check ErrorCode as a plain string too, this is just for the known set.
public static class SyncVpnErrorCodes
{
    public const string UserBlocked = "USER_BLOCKED";
    public const string SystemMaintenance = "SYSTEM_MAINTENANCE";
    public const string AccountNotFound = "ACCOUNT_NOT_FOUND";
    public const string ProServerRequired = "PRO_SERVER_REQUIRED";
    public const string AccountLimitReached = "ACCOUNT_LIMIT_REACHED";
    public const string CityNotFound = "CITY_NOT_FOUND";
    public const string NoAvailableServerInCity = "NO_AVAILABLE_SERVER_IN_CITY";
    public const string PurchaseAlreadyCompleted = "PURCHASE_ALREADY_COMPLETED";
    public const string SalesDisabled = "SALES_DISABLED";
    public const string RenewalsDisabled = "RENEWALS_DISABLED";

    // POST /checkout-links 409 codes.
    public const string PlanUnavailable = "PLAN_UNAVAILABLE";
    public const string PurchaseIdentityConflict = "PURCHASE_IDENTITY_CONFLICT";
    public const string InvalidRenewal = "INVALID_RENEWAL";
    public const string ActiveSubscriptionExists = "ACTIVE_SUBSCRIPTION_EXISTS";
    public const string RenewalRequired = "RENEWAL_REQUIRED";
}
