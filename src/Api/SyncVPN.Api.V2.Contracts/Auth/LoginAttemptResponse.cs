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

namespace SyncVPN.Api.V2.Contracts.Auth;

// /auth/login and /auth/code-login return either a 200 (Login, DeviceToken issued) or a 202
// (Challenge, 2FA required, no DeviceToken yet) - this wraps both outcomes since ApiResponseResult<T>
// is single-typed and both are "success" from the transport's point of view.
public class LoginAttemptResponse
{
    public bool RequiresTwoFactor { get; set; }

    public LoginResponse? Login { get; set; }

    public TwoFactorChallengeResponse? Challenge { get; set; }
}
