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
using SyncVPN.Api.V2.Contracts.Devices;
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

    // A bad/zero server-provided poll_interval must never turn into a tight poll loop.
    private static readonly TimeSpan MinWebLoginPollInterval = TimeSpan.FromSeconds(1);

    public async Task<WebLoginStartResult> StartWebLoginAsync(CancellationToken cancellationToken)
    {
        ApiResponseResult<WebAppLoginResponse> response = await _apiClient.StartWebLoginAsync(cancellationToken);

        if (!response.Success || response.Value?.Data is not { } data || string.IsNullOrEmpty(data.PollToken))
        {
            return WebLoginStartResult.FromAuthResult(AuthResult.Fail(response.Error));
        }

        TimeSpan pollInterval = data.PollInterval > 0 ? TimeSpan.FromSeconds(data.PollInterval) : MinWebLoginPollInterval;
        return WebLoginStartResult.Ok(data.VerificationUrl, data.Key, data.PollToken, pollInterval);
    }

    public async Task<AuthResult> WaitForWebLoginAsync(WebLoginStartResult attempt, CancellationToken cancellationToken)
    {
        TimeSpan pollInterval = attempt.PollInterval < MinWebLoginPollInterval ? MinWebLoginPollInterval : attempt.PollInterval;

        while (true)
        {
            ApiResponseResult<WebAppLoginStatusResponse> response =
                await _apiClient.GetWebLoginStatusAsync(attempt.Key, attempt.PollToken, cancellationToken);

            if (!response.Success)
            {
                return AuthResult.Fail(response.Error);
            }

            if (string.Equals(response.Value?.Data?.State, "authorized", StringComparison.OrdinalIgnoreCase) &&
                response.Value?.Data is { Token.Length: > 0 } authorizedData)
            {
                StoreSession(new LoginResponse
                {
                    Status = true,
                    Data = new LoginResponseData
                    {
                        Token = authorizedData.Token!,
                        TokenType = authorizedData.TokenType ?? string.Empty,
                        User = authorizedData.User ?? new User(),
                        Device = authorizedData.Device ?? new Device(),
                        Devices = authorizedData.Devices ?? [],
                    }
                });
                return AuthResult.Ok();
            }

            await Task.Delay(pollInterval, cancellationToken);
        }
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
        if (!response.Success || response.Value is null)
        {
            ClearSession();
            return AuthResult.Fail(AuthError.GetSessionDetailsFailed, response.Error);
        }

        // Refresh identity in case it changed server-side (name/email/phone/address edited elsewhere) -
        // ReferralCode is deliberately left untouched here, see StoreIdentity.
        StoreIdentity(response.Value.Data.User, includeReferralCode: false);

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
        StoreIdentity(login.Data.User, includeReferralCode: true);
    }

    // ReferralCode is only ever populated from a login response - the API contract only documents it as
    // "always present" there, not on GET /auth/me, so ValidateSessionAsync's refresh leaves it alone
    // rather than risking silently blanking a real code out with an absent/empty field.
    private void StoreIdentity(User user, bool includeReferralCode)
    {
        _settings.SyncVpnUserName = user.Name;
        _settings.SyncVpnUserEmail = user.Email;
        _settings.SyncVpnUserPhone = user.Phone;
        _settings.SyncVpnUserAddress = user.Address;

        if (includeReferralCode)
        {
            _settings.SyncVpnReferralCode = user.ReferralCode;
        }
    }

    private void ClearSession()
    {
        _settings.SyncVpnDeviceToken = null;
        _settings.SyncVpnUserName = null;
        _settings.SyncVpnUserEmail = null;
        _settings.SyncVpnUserPhone = null;
        _settings.SyncVpnUserAddress = null;
        _settings.SyncVpnReferralCode = null;
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
