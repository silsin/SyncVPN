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

using SyncVPN.Client.Logic.Purchases.Contracts.Models;

namespace SyncVPN.Client.Logic.Purchases.Contracts;

// Orchestrates POST /checkout-links: for a logged-in device this just needs a plan id (the backend
// extracts the account's own email and attaches the request to it via the Bearer that
// SyncVpnApiClient.CreateRequest already sends); a guest device has no account email, so the caller
// must collect one and pass it as guestEmail - the server rejects a guest request with no email.
//
// Only action=purchase is supported here - nothing in this app has a renewal UI yet, and action=renew
// requires a logged-in device's own Bearer plus different validation this service doesn't model.
public interface ICheckoutLinkService
{
    Task<CheckoutLinkResult> CreatePurchaseLinkAsync(long planId, string currencyCode, string? guestEmail, CancellationToken cancellationToken = default);

    // Polls POST /checkout-links/status at pollIntervalSeconds (from the create response) until a
    // terminal state (completed/expired/failed/refunded), exchanging a completed guest purchase's
    // returned credentials for a session before returning. Never times out on its own - cancel
    // cancellationToken to stop waiting (e.g. the user closed the "waiting for payment" dialog).
    Task<CheckoutCompletionResult> WaitForCompletionAsync(string key, string pollToken, int pollIntervalSeconds, CancellationToken cancellationToken = default);
}
