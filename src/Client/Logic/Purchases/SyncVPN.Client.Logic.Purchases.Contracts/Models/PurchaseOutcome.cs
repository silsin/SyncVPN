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

namespace SyncVPN.Client.Logic.Purchases.Contracts.Models;

public enum PurchaseOutcome
{
    // Purchase verified (fresh or replayed) and, if a login_code was returned, it was exchanged for
    // a device session successfully. The user now has Pro access; connecting to a Pro server through
    // the normal connection flow provisions the VPN account (see BackendCapability.VpnProvisioning).
    Completed,

    // The purchase itself succeeded, but exchanging the returned login_code for a session failed
    // (e.g. transient network error). The purchase is not lost - retrying LoginWithCodeAsync with the
    // same code, or resubmitting the purchase with the same idempotency key, recovers it.
    LoginFailed,

    // 409 PURCHASE_ALREADY_COMPLETED (or an equivalent conflict) - this purchase identity/receipt was
    // already used, by this device or another.
    AlreadyCompleted,

    // 503 SALES_DISABLED - new purchases are temporarily halted.
    SalesDisabled,

    // 503 RENEWALS_DISABLED - plan renewals are temporarily halted.
    RenewalsDisabled,

    // 403 - this user/device is blocked.
    Blocked,

    // Any other failure (422 validation, 401, 429 rate limit, network error, etc).
    Failed,
}
