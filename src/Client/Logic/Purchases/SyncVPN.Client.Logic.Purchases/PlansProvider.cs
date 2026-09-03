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
using SyncVPN.Api.V2.Contracts.Plans;
using SyncVPN.Client.Logic.Purchases.Contracts;

namespace SyncVPN.Client.Logic.Purchases;

public class PlansProvider : IPlansProvider
{
    private readonly ISyncVpnApiClient _apiClient;

    public PlansProvider(ISyncVpnApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponseResult<PlanListResponse>> GetPlansAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetPlansAsync(cancellationToken);
    }
}
