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
using SyncVPN.Api.V2.Contracts.Billing;

namespace SyncVPN.Client.Logic.Purchases.Contracts;

// GET /billing/countries - purchase-eligible countries with VAT/tax rates, for the billing-country
// picker in the purchase UI.
public interface IBillingCountriesProvider
{
    Task<ApiResponseResult<BillingCountryListResponse>> GetBillingCountriesAsync(CancellationToken cancellationToken = default);
}
