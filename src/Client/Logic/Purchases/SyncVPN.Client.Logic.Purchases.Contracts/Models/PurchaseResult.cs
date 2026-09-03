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

using SyncVPN.Api.V2.Contracts.Common;
using SyncVPN.Api.V2.Contracts.Purchases;

namespace SyncVPN.Client.Logic.Purchases.Contracts.Models;

public class PurchaseResult
{
    public required PurchaseOutcome Outcome { get; init; }

    // Populated when Outcome is Completed or LoginFailed.
    public PurchaseResponse? Response { get; init; }

    // Populated when the backend returned a structured error body.
    public SyncVpnErrorResponse? Error { get; init; }

    // Populated when the failure had no structured body to parse (e.g. a transport error).
    public string? RawError { get; init; }

    public static PurchaseResult Completed(PurchaseResponse response)
    {
        return new PurchaseResult { Outcome = PurchaseOutcome.Completed, Response = response };
    }

    public static PurchaseResult LoginFailed(PurchaseResponse response, SyncVpnErrorResponse? loginError, string? rawLoginError)
    {
        return new PurchaseResult { Outcome = PurchaseOutcome.LoginFailed, Response = response, Error = loginError, RawError = rawLoginError };
    }

    public static PurchaseResult Fail(PurchaseOutcome outcome, SyncVpnErrorResponse? error, string? rawError)
    {
        return new PurchaseResult { Outcome = outcome, Error = error, RawError = rawError };
    }
}
