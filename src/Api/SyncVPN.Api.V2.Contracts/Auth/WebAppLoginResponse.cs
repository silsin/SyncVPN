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

// 201 response from POST /auth/web-app - starts a browser-based login attempt for this device.
public class WebAppLoginResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("requires_login")]
    public bool RequiresLogin { get; set; }

    [JsonProperty("data")]
    public WebAppLoginData? Data { get; set; }
}

public class WebAppLoginData
{
    // Public identifier for this login attempt/link - safe to pass to the browser and to log.
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    // Open exactly this URL in the browser - never reconstruct or append to it client-side.
    [JsonProperty("verification_url")]
    public string VerificationUrl { get; set; } = string.Empty;

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("poll_interval")]
    public int PollInterval { get; set; }

    // Confidential - authorizes reading this attempt's result via POST /auth/web-app/status. Never log
    // it, put it in a URL, or pass it to the browser; only send it in that endpoint's JSON body.
    [JsonProperty("poll_token")]
    public string PollToken { get; set; } = string.Empty;
}
