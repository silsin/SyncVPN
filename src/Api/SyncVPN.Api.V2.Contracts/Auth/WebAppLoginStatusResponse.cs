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
using SyncVPN.Api.V2.Contracts.Devices;

namespace SyncVPN.Api.V2.Contracts.Auth;

// Both the 200 (authorized) and 202 (still pending) success responses from POST /auth/web-app/status
// share this shape - Data.State tells them apart, and the login fields are only populated once it's
// "authorized". 403/404/409/410/422/429/503 are all non-success status codes and never reach here -
// SyncVpnApiClient.ReadResponseAsync surfaces those as a plain failure with the raw body as the error.
public class WebAppLoginStatusResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("requires_login")]
    public bool RequiresLogin { get; set; }

    [JsonProperty("requires_two_factor")]
    public bool RequiresTwoFactor { get; set; }

    [JsonProperty("data")]
    public WebAppLoginStatusData? Data { get; set; }
}

public class WebAppLoginStatusData
{
    // "pending" or "authorized".
    [JsonProperty("state")]
    public string State { get; set; } = string.Empty;

    // A live remaining-seconds countdown while pending - observed as a non-integer value (e.g.
    // 285.658...), unlike the integer expires_in on the initiate response (WebAppLoginData).
    [JsonProperty("expires_in")]
    public double? ExpiresIn { get; set; }

    // The rest are only present once State is "authorized".
    [JsonProperty("token")]
    public string? Token { get; set; }

    [JsonProperty("token_type")]
    public string? TokenType { get; set; }

    [JsonProperty("user")]
    public User? User { get; set; }

    [JsonProperty("device")]
    public Device? Device { get; set; }

    [JsonProperty("devices")]
    public List<Device>? Devices { get; set; }
}
