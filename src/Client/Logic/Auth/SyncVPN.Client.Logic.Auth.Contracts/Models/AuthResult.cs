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

using SyncVPN.Api.Contracts;
using SyncVPN.Api.Contracts.Common;
using SyncVPN.Client.Logic.Auth.Contracts.Enums;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Common.Legacy.Abstract;

namespace SyncVPN.Client.Logic.Auth.Contracts.Models;

public class AuthResult : Result<AuthError>
{
    protected internal AuthResult(AuthError value, bool success, string error) 
        : base(value, success, error)
    {
    }

    public static AuthResult Fail(AuthError authError)
    {
        return new(authError, false, string.Empty);
    }

    public static AuthResult Fail(AuthError authError, string error)
    {
        return new(authError, false, error);
    }

    public new static AuthResult Fail(string error)
    {
        return new(AuthError.Unknown, false, error);
    }

    public static AuthResult Fail<T>(ApiResponseResult<T> apiResponseResult) where T : BaseResponse
    {
        if (apiResponseResult.Value?.Code == ResponseCodes.NO_VPN_CONNECTIONS_ASSIGNED)
        {
            return Fail(AuthError.NoVpnAccess, apiResponseResult.Error);
        }

        if (apiResponseResult.Actions.IsNullOrEmpty())
        {
            return apiResponseResult.Value?.Code switch
            {
                ResponseCodes.NO_VPN_CONNECTIONS_ASSIGNED => Fail(AuthError.NoVpnAccess, apiResponseResult.Error),
                ResponseCodes.AUTH_SWITCH_TO_SSO => Fail(AuthError.SwitchToSSO, apiResponseResult.Error),
                ResponseCodes.AUTH_SWITCH_TO_SRP => Fail(AuthError.SwitchToSRP, apiResponseResult.Error),
                _ => Fail(apiResponseResult.Error)
            };
        }

        return Fail();
    }

    public static AuthResult Fail()
    {
        return new(AuthError.None, false, string.Empty);
    }

    public new static AuthResult Ok()
    {
        return new(AuthError.None, true, string.Empty);
    }
}