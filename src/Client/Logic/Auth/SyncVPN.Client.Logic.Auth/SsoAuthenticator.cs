/*
 * Copyright (c) 2025 Proton AG
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
using SyncVPN.Api.Contracts.Auth;
using SyncVPN.Client.Logic.Auth.Contracts.Enums;
using SyncVPN.Client.Logic.Auth.Contracts.Models;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.UserLogs;

namespace SyncVPN.Client.Logic.Auth;

public class SsoAuthenticator : AuthenticatorBase, ISsoAuthenticator
{
    private const string SSO_LOGIN_INTENT = "SSO";

    private readonly IApiClient _apiClient;
    private readonly ILogger _logger;
    private readonly IUnauthSessionManager _unauthSessionManager;

    public SsoAuthenticator(
        IApiClient apiClient,
        ISettings settings,
        ILogger logger,
        IUnauthSessionManager unauthSessionManager) : base(settings)
    {
        _apiClient = apiClient;
        _logger = logger;
        _unauthSessionManager = unauthSessionManager;
    }

    public async Task<SsoAuthResult> StartSsoAuthAsync(string username, CancellationToken cancellationToken)
    {
        await _unauthSessionManager.CreateIfDoesNotExistAsync(cancellationToken);

        AuthInfoRequest infoRequest = new()
        {
            Username = username,
            Intent = SSO_LOGIN_INTENT
        };

        ApiResponseResult<AuthInfoResponse> authInfoResponse = await _apiClient.GetAuthInfoResponse(infoRequest, cancellationToken);
        if (!authInfoResponse.Success || string.IsNullOrEmpty(authInfoResponse.Value?.SsoChallengeToken))
        {
            _logger.Error<UserLog>("Failed to login with SSO.");
            return SsoAuthResult.FromAuthResult(AuthResult.Fail(authInfoResponse));
        }

        return SsoAuthResult.Ok(authInfoResponse.Value.SsoChallengeToken);
    }

    public async Task<AuthResult> CompleteSsoAuthAsync(string ssoResponseToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(ssoResponseToken))
        {
            return AuthResult.Fail(AuthError.SsoAuthFailed);
        }

        AuthRequest authRequest = new()
        {
            SsoResponseToken = ssoResponseToken,
        };

        ApiResponseResult<AuthResponse> authResponse = await _apiClient.GetAuthResponse(authRequest, cancellationToken);
        if (authResponse.Failure)
        {
            return AuthResult.Fail(authResponse);
        }

        SaveAuthSessionDetails(authResponse.Value);

        return AuthResult.Ok();
    }
}