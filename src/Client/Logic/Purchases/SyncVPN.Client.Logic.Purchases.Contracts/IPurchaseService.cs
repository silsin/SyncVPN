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

using SyncVPN.Api.V2.Contracts.Purchases;
using SyncVPN.Client.Logic.Purchases.Contracts.Models;

namespace SyncVPN.Client.Logic.Purchases.Contracts;

// Orchestrates POST /purchases end to end: submits the receipt, and - if the purchase was made as a
// guest and returned a login_code - exchanges it for a device session via IUserAuthenticator, so the
// caller ends up with either a fully logged-in Pro session or a clear, typed failure reason.
//
// Deliberately does NOT pick a server or call POST /account itself: once the user has Pro access,
// connecting to any Pro server through the normal connection flow provisions the VPN account
// automatically (see ConnectionRequestCreator.ClaimAccountAsync, gated by
// BackendCapability.VpnProvisioning) - there is no separate "provisioning" step to orchestrate here.
public interface IPurchaseService
{
    Task<PurchaseResult> SubmitPurchaseAsync(PurchaseRequest request, CancellationToken cancellationToken = default);
}
