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

// Returned as HTTP 202 by /auth/login and /auth/code-login when the account has Google Authenticator
// enabled - no DeviceToken yet, the app must call /auth/2fa/verify with this challenge token + a
// current 6-digit TOTP code before one is issued.
public class TwoFactorChallengeResponseData
{
    [JsonProperty("two_factor_token")]
    public string TwoFactorToken { get; set; } = string.Empty;

    [JsonProperty("code_length")]
    public int CodeLength { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonProperty("enrollment_required")]
    public bool EnrollmentRequired { get; set; }
}

public class TwoFactorChallengeResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("requires_login")]
    public bool RequiresLogin { get; set; }

    [JsonProperty("requires_two_factor")]
    public bool RequiresTwoFactor { get; set; }

    [JsonProperty("next_step")]
    public string NextStep { get; set; } = string.Empty;

    [JsonProperty("data")]
    public TwoFactorChallengeResponseData Data { get; set; } = new();

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;
}
