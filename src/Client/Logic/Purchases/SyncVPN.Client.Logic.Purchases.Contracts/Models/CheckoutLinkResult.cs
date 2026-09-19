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

public class CheckoutLinkResult
{
    public required CheckoutLinkOutcome Outcome { get; init; }

    // Populated when Outcome is Created.
    public CheckoutLinkResponseData? Data { get; init; }

    // Populated when the backend returned a structured error body.
    public SyncVpnErrorResponse? Error { get; init; }

    // Populated when the failure had no structured body to parse (e.g. a transport error).
    public string? RawError { get; init; }

    public static CheckoutLinkResult Created(CheckoutLinkResponseData data)
    {
        return new CheckoutLinkResult { Outcome = CheckoutLinkOutcome.Created, Data = data };
    }

    public static CheckoutLinkResult Fail(CheckoutLinkOutcome outcome, SyncVpnErrorResponse? error, string? rawError)
    {
        return new CheckoutLinkResult { Outcome = outcome, Error = error, RawError = rawError };
    }
}
