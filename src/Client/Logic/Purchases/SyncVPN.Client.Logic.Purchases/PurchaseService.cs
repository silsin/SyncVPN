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
using SyncVPN.Api.V2.Contracts.Common;
using SyncVPN.Api.V2.Contracts.Purchases;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Models;
using SyncVPN.Client.Logic.Purchases.Contracts;
using SyncVPN.Client.Logic.Purchases.Contracts.Models;

namespace SyncVPN.Client.Logic.Purchases;

public class PurchaseService : IPurchaseService
{
    private readonly ISyncVpnApiClient _apiClient;
    private readonly IUserAuthenticator _userAuthenticator;

    public PurchaseService(ISyncVpnApiClient apiClient, IUserAuthenticator userAuthenticator)
    {
        _apiClient = apiClient;
        _userAuthenticator = userAuthenticator;
    }

    public async Task<PurchaseResult> SubmitPurchaseAsync(PurchaseRequest request, CancellationToken cancellationToken = default)
    {
        ApiResponseResult<PurchaseResponse> response = await _apiClient.SubmitPurchaseAsync(request, cancellationToken);

        if (!response.Success || response.Value is null)
        {
            return MapFailure(response.Error);
        }

        PurchaseResponse purchase = response.Value;

        if (!purchase.RequiresLogin || string.IsNullOrEmpty(purchase.Data.LoginCode))
        {
            // Already-logged-in purchase, or a replay that didn't need a new session.
            return PurchaseResult.Completed(purchase);
        }

        AuthResult loginResult = await _userAuthenticator.LoginWithCodeAsync(purchase.Data.LoginCode);

        return loginResult.Success
            ? PurchaseResult.Completed(purchase)
            : PurchaseResult.LoginFailed(purchase, SyncVpnErrorResponse.TryParse(loginResult.Error), loginResult.Error);
    }

    private static PurchaseResult MapFailure(string? rawError)
    {
        SyncVpnErrorResponse? error = SyncVpnErrorResponse.TryParse(rawError);

        PurchaseOutcome outcome = error?.ErrorCode switch
        {
            SyncVpnErrorCodes.PurchaseAlreadyCompleted => PurchaseOutcome.AlreadyCompleted,
            SyncVpnErrorCodes.SalesDisabled => PurchaseOutcome.SalesDisabled,
            SyncVpnErrorCodes.RenewalsDisabled => PurchaseOutcome.RenewalsDisabled,
            SyncVpnErrorCodes.UserBlocked => PurchaseOutcome.Blocked,
            _ => error?.Blocked == true ? PurchaseOutcome.Blocked : PurchaseOutcome.Failed,
        };

        return PurchaseResult.Fail(outcome, error, rawError);
    }
}
