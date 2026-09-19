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

using SyncVPN.Api.V2.Contracts.CheckoutLinks;
using SyncVPN.Api.V2.Contracts.Common;

namespace SyncVPN.Client.Logic.Purchases.Contracts.Models;

public class CheckoutCompletionResult
{
    public required CheckoutCompletionOutcome Outcome { get; init; }

    // Populated for every outcome except Error (a polling/transport failure has no status body).
    public CheckoutStatusData? Status { get; init; }

    // Populated when the backend returned a structured error body (Error outcome), or when the
    // guest login exchange itself failed with one (LoginFailed outcome).
    public SyncVpnErrorResponse? Error { get; init; }

    public string? RawError { get; init; }

    public static CheckoutCompletionResult Completed(CheckoutStatusData status)
    {
        return new CheckoutCompletionResult { Outcome = CheckoutCompletionOutcome.Completed, Status = status };
    }

    public static CheckoutCompletionResult LoginFailed(CheckoutStatusData status, SyncVpnErrorResponse? error, string? rawError)
    {
        return new CheckoutCompletionResult { Outcome = CheckoutCompletionOutcome.LoginFailed, Status = status, Error = error, RawError = rawError };
    }

    public static CheckoutCompletionResult Terminal(CheckoutCompletionOutcome outcome, CheckoutStatusData status)
    {
        return new CheckoutCompletionResult { Outcome = outcome, Status = status };
    }

    public static CheckoutCompletionResult Fail(SyncVpnErrorResponse? error, string? rawError)
    {
        return new CheckoutCompletionResult { Outcome = CheckoutCompletionOutcome.Error, Error = error, RawError = rawError };
    }
}
