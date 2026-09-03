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
using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts.Dns;

namespace SyncVPN.Client.Services.DnsFilters;

// Thin wrapper around ISyncVpnApiClient's DNS-filter endpoints, gated by BackendCapability.DnsFilters.
// This is a net-new feature (no equivalent in the app today - NetShield is a different, client-side
// mechanism) so there's no legacy path to branch around, unlike ApiClient's migrated methods.
public interface IDnsFiltersManager
{
    // True only when the new backend is enabled for this capability - the settings page and its entry
    // tile should be hidden entirely when this is false.
    bool IsAvailable { get; }

    Task<ApiResponseResult<AccountDnsFilterResponse>> GetDnsFiltersAsync(CancellationToken cancellationToken = default);

    Task<ApiResponseResult<UpdateAccountDnsFiltersResponse>> UpdateDnsFiltersAsync(DnsFilterPatch patch, CancellationToken cancellationToken = default);
}
