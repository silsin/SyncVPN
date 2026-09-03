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

using System.Threading;
using System.Threading.Tasks;
using SyncVPN.Api.BackendSelection;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts;
using SyncVPN.Api.V2.Contracts.Dns;

namespace SyncVPN.Client.Services.DnsFilters;

public class DnsFiltersManager : IDnsFiltersManager
{
    private readonly ISyncVpnApiClient _apiClient;
    private readonly IBackendModeProvider _backendModeProvider;

    public DnsFiltersManager(ISyncVpnApiClient apiClient, IBackendModeProvider backendModeProvider)
    {
        _apiClient = apiClient;
        _backendModeProvider = backendModeProvider;
    }

    public bool IsAvailable => _backendModeProvider.IsNewBackendEnabled(BackendCapability.DnsFilters);

    public Task<ApiResponseResult<AccountDnsFilterResponse>> GetDnsFiltersAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetDnsFiltersAsync(cancellationToken);
    }

    public Task<ApiResponseResult<UpdateAccountDnsFiltersResponse>> UpdateDnsFiltersAsync(DnsFilterPatch patch, CancellationToken cancellationToken = default)
    {
        return _apiClient.UpdateDnsFiltersAsync(patch, cancellationToken);
    }
}
