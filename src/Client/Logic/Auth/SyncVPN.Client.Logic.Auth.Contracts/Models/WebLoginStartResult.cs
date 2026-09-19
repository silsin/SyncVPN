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

using SyncVPN.Client.Logic.Auth.Contracts.Enums;

namespace SyncVPN.Client.Logic.Auth.Contracts.Models;

// Result of starting a browser-based login attempt (IUserAuthenticator.StartWebLoginAsync). On success,
// carries everything needed to open the browser and then poll for the result via WaitForWebLoginAsync.
public class WebLoginStartResult : AuthResult
{
    public string VerificationUrl { get; init; } = string.Empty;

    public string Key { get; init; } = string.Empty;

    // Confidential - authorizes reading this attempt's result. Never log it, put it in a URL, or pass
    // it to the browser; WaitForWebLoginAsync only ever sends it in a POST body.
    public string PollToken { get; init; } = string.Empty;

    public TimeSpan PollInterval { get; init; }

    protected internal WebLoginStartResult(AuthError value, bool success, string error)
        : base(value, success, error)
    {
    }

    public static WebLoginStartResult FromAuthResult(AuthResult result)
    {
        return new(result.Value, result.Success, result.Error);
    }

    public static WebLoginStartResult Ok(string verificationUrl, string key, string pollToken, TimeSpan pollInterval)
    {
        return new(AuthError.None, true, string.Empty)
        {
            VerificationUrl = verificationUrl,
            Key = key,
            PollToken = pollToken,
            PollInterval = pollInterval,
        };
    }
}
