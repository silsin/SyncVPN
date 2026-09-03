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
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts;
using SyncVPN.Api.V2.Contracts.Auth;
using SyncVPN.Client.Logic.Auth.Contracts.Enums;
using SyncVPN.Client.Logic.Auth.Contracts.Models;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.UserLogs;

namespace SyncVPN.Client.Logic.Auth;

public class SyncVpnAuthenticator : ISyncVpnAuthenticator
{
    private readonly ISyncVpnApiClient _apiClient;
    private readonly ISettings _settings;
    private readonly ILogger _logger;

    // Carries the 2FA challenge token between LoginUserAsync and SendTwoFactorCodeAsync - short-lived
    // (10 minutes per the API contract), never persisted.
    private string? _twoFactorToken;

    public SyncVpnAuthenticator(ISyncVpnApiClient apiClient, ISettings settings, ILogger logger)
    {
        _apiClient = apiClient;
        _settings = settings;
        _logger = logger;
    }

    public bool HasAuthenticatedSessionData()
    {
        return !string.IsNullOrWhiteSpace(_settings.SyncVpnDeviceToken);
    }

    public async Task<AuthResult> LoginUserAsync(string username, SecureString password, CancellationToken cancellationToken)
    {
        string plainPassword = ToPlainString(password);

        LoginRequest request = new() { Email = username, Password = plainPassword };
        ApiResponseResult<LoginAttemptResponse> response = await _apiClient.LoginAsync(request, cancellationToken);

        return HandleLoginAttempt(response);
    }

    public async Task<AuthResult> LoginWithCodeAsync(string code, CancellationToken cancellationToken)
    {
        CodeLoginRequest request = new() { Code = code };
        ApiResponseResult<LoginAttemptResponse> response = await _apiClient.CodeLoginAsync(request, cancellationToken);

        return HandleLoginAttempt(response);
    }

    public async Task<AuthResult> SendTwoFactorCodeAsync(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_twoFactorToken))
        {
            return AuthResult.Fail(AuthError.TwoFactorCancelled);
        }

        TwoFactorVerifyRequest request = new() { TwoFactorToken = _twoFactorToken, Code = code };
        ApiResponseResult<LoginResponse> response = await _apiClient.VerifyTwoFactorAsync(request, cancellationToken);

        _twoFactorToken = null;

        if (!response.Success || response.Value is null)
        {
            return AuthResult.Fail(AuthError.IncorrectTwoFactorCode, response.Error);
        }

        StoreSession(response.Value);
        return AuthResult.Ok();
    }

    public async Task<AuthResult> ValidateSessionAsync(CancellationToken cancellationToken)
    {
        if (!HasAuthenticatedSessionData())
        {
            return AuthResult.Fail(AuthError.GetSessionDetailsFailed);
        }

        ApiResponseResult<AuthenticatedDeviceResponse> response = await _apiClient.GetAuthenticatedDeviceAsync(cancellationToken);
        if (!response.Success)
        {
            ClearSession();
            return AuthResult.Fail(AuthError.GetSessionDetailsFailed, response.Error);
        }

        return AuthResult.Ok();
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.LogoutAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.Error<UserLog>("An error occurred when sending a SyncVPN logout request.", e);
        }
        finally
        {
            ClearSession();
        }
    }

    private AuthResult HandleLoginAttempt(ApiResponseResult<LoginAttemptResponse> response)
    {
        if (!response.Success || response.Value is null)
        {
            return AuthResult.Fail(response.Error);
        }

        if (response.Value.RequiresTwoFactor)
        {
            _twoFactorToken = response.Value.Challenge?.Data.TwoFactorToken;
            return AuthResult.Fail(AuthError.TwoFactorRequired);
        }

        if (response.Value.Login is null)
        {
            return AuthResult.Fail(AuthError.Unknown);
        }

        StoreSession(response.Value.Login);
        return AuthResult.Ok();
    }

    private void StoreSession(LoginResponse login)
    {
        _settings.SyncVpnDeviceToken = login.Data.Token;
    }

    private void ClearSession()
    {
        _settings.SyncVpnDeviceToken = null;
    }

    private static string ToPlainString(SecureString secureString)
    {
        IntPtr unmanagedString = IntPtr.Zero;
        try
        {
            unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            return Marshal.PtrToStringUni(unmanagedString) ?? string.Empty;
        }
        finally
        {
            if (unmanagedString != IntPtr.Zero)
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
