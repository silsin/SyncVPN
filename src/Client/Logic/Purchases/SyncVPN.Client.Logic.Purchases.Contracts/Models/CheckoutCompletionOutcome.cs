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

public enum CheckoutCompletionOutcome
{
    // Payment verified. For a guest purchase with usable credentials, login also succeeded (or wasn't
    // needed - a logged-in device's own purchase needs no exchange at all).
    Completed,

    // Payment verified, but exchanging the returned login_code/email+password for a session failed.
    LoginFailed,

    Expired,
    Failed,
    Refunded,

    // Polling itself failed (network error, or the backend returned a non-success response).
    Error,
}
