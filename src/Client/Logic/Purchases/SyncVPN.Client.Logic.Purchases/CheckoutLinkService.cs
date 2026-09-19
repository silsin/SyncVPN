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
using SyncVPN.Api.V2.Contracts;
using SyncVPN.Api.V2.Contracts.CheckoutLinks;
using SyncVPN.Api.V2.Contracts.Common;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Models;
using SyncVPN.Client.Logic.Purchases.Contracts;
using SyncVPN.Client.Logic.Purchases.Contracts.Models;
using SyncVPN.Common.Core.Extensions;

namespace SyncVPN.Client.Logic.Purchases;

public class CheckoutLinkService : ICheckoutLinkService
{
    // A bad/zero server-provided poll_interval must never turn into a tight poll loop - same guard
    // SyncVpnAuthenticator.WaitForWebLoginAsync uses for its own status-polling loop.
    private static readonly TimeSpan MinPollInterval = TimeSpan.FromSeconds(1);

    private readonly ISyncVpnApiClient _apiClient;
    private readonly IUserAuthenticator _userAuthenticator;

    public CheckoutLinkService(ISyncVpnApiClient apiClient, IUserAuthenticator userAuthenticator)
    {
        _apiClient = apiClient;
        _userAuthenticator = userAuthenticator;
    }

    public async Task<CheckoutLinkResult> CreatePurchaseLinkAsync(long planId, string currencyCode, string? guestEmail, CancellationToken cancellationToken = default)
    {
        CheckoutLinkRequest request = new()
        {
            PlanId = planId,
            Action = CheckoutLinkActions.Purchase,
            // The backend extracts and uses the account's own email for a logged-in device and ignores
            // whatever is sent here, so only a guest (who has no account email) needs to supply one.
            Email = _userAuthenticator.IsLoggedIn ? null : guestEmail,
            Currency = currencyCode,
        };

        ApiResponseResult<CheckoutLinkResponse> response = await _apiClient.CreateCheckoutLinkAsync(request, cancellationToken);

        return !response.Success || response.Value?.Data is null
            ? MapFailure(response.Error)
            : CheckoutLinkResult.Created(response.Value.Data);
    }

    private static CheckoutLinkResult MapFailure(string? rawError)
    {
        SyncVpnErrorResponse? error = SyncVpnErrorResponse.TryParse(rawError);

        CheckoutLinkOutcome outcome = error switch
        {
            { RequiresEmail: true } => CheckoutLinkOutcome.RequiresEmail,
            { RequiresLogin: true } => CheckoutLinkOutcome.RequiresLogin,
            { Blocked: true } => CheckoutLinkOutcome.Blocked,
            { ErrorCode: SyncVpnErrorCodes.PlanUnavailable } => CheckoutLinkOutcome.PlanUnavailable,
            { ErrorCode: SyncVpnErrorCodes.ActiveSubscriptionExists } => CheckoutLinkOutcome.ActiveSubscriptionExists,
            { ErrorCode: SyncVpnErrorCodes.SalesDisabled or SyncVpnErrorCodes.RenewalsDisabled } => CheckoutLinkOutcome.SalesDisabled,
            _ => CheckoutLinkOutcome.Failed,
        };

        return CheckoutLinkResult.Fail(outcome, error, rawError);
    }

    public async Task<CheckoutCompletionResult> WaitForCompletionAsync(string key, string pollToken, int pollIntervalSeconds, CancellationToken cancellationToken = default)
    {
        TimeSpan pollInterval = pollIntervalSeconds > 0 ? TimeSpan.FromSeconds(pollIntervalSeconds) : MinPollInterval;
        CheckoutStatusRequest request = new() { Key = key, PollToken = pollToken };

        while (true)
        {
            ApiResponseResult<CheckoutStatusResponse> response = await _apiClient.GetCheckoutLinkStatusAsync(request, cancellationToken);

            if (!response.Success || response.Value?.Data is null)
            {
                return CheckoutCompletionResult.Fail(SyncVpnErrorResponse.TryParse(response.Error), response.Error);
            }

            CheckoutStatusData data = response.Value.Data;

            switch (data.State)
            {
                case CheckoutLinkStates.Completed:
                    return await CompleteAsync(response.Value, data);
                case CheckoutLinkStates.Expired:
                    return CheckoutCompletionResult.Terminal(CheckoutCompletionOutcome.Expired, data);
                case CheckoutLinkStates.Failed:
                    return CheckoutCompletionResult.Terminal(CheckoutCompletionOutcome.Failed, data);
                case CheckoutLinkStates.Refunded:
                    return CheckoutCompletionResult.Terminal(CheckoutCompletionOutcome.Refunded, data);
                default:
                    // pending / confirming (possibly with VerificationPending for a crypto payment) - keep waiting.
                    break;
            }

            await Task.Delay(pollInterval, cancellationToken);
        }
    }

    private async Task<CheckoutCompletionResult> CompleteAsync(CheckoutStatusResponse response, CheckoutStatusData data)
    {
        if (!response.RequiresLogin || data.Authentication is null)
        {
            // Either a logged-in device's own purchase, or a guest purchase past the 24h/validity
            // window for exchanging credentials - nothing more to do here either way.
            return CheckoutCompletionResult.Completed(data);
        }

        AuthResult loginResult = await ExchangeGuestCredentialsAsync(data.Authentication);

        return loginResult.Success
            ? CheckoutCompletionResult.Completed(data)
            : CheckoutCompletionResult.LoginFailed(data, SyncVpnErrorResponse.TryParse(loginResult.Error), loginResult.Error);
    }

    private Task<AuthResult> ExchangeGuestCredentialsAsync(CheckoutStatusAuthentication authentication)
    {
        if (!string.IsNullOrEmpty(authentication.LoginCode))
        {
            return _userAuthenticator.LoginWithCodeAsync(authentication.LoginCode);
        }

        if (!string.IsNullOrEmpty(authentication.Email) && !string.IsNullOrEmpty(authentication.Password))
        {
            return _userAuthenticator.LoginUserAsync(authentication.Email, authentication.Password.ToSecureString());
        }

        return Task.FromResult(AuthResult.Fail("The completed purchase did not include usable sign-in credentials."));
    }
}
