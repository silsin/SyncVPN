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

public enum CheckoutLinkOutcome
{
    // 201 - PaymentUrl is ready to open in the browser.
    Created,

    // 422 requires_email - the request needs a guest email that wasn't supplied (or was rejected).
    RequiresEmail,

    // 401 - invalid/missing Bearer for a call that needed one (e.g. a guest attempting action=renew).
    RequiresLogin,

    // 403, or blocked:true - this user/device is blocked.
    Blocked,

    // 409 PLAN_UNAVAILABLE - the requested plan can no longer be purchased.
    PlanUnavailable,

    // 409 ACTIVE_SUBSCRIPTION_EXISTS - the user already has an active subscription for this action.
    ActiveSubscriptionExists,

    // 503 SALES_DISABLED or RENEWALS_DISABLED - temporarily halted, retryable per the response.
    SalesDisabled,

    // Any other failure (other 409 codes, 404 bad app token, 429 rate limit, network error, etc).
    Failed,
}
