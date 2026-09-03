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
using SyncVPN.Api.V2.Contracts.Servers;

namespace SyncVPN.Client.Logic.Purchases.Contracts;

// GET /servers - the new backend's free-server catalog, for a standalone free-servers list. Distinct
// from IServersLoader (SyncVPN.Client.Logic.Servers.Contracts), which owns the legacy Proton
// Physical/Logical server model that the existing sidebar/search/map UI renders - the two models
// aren't compatible (this one has no Tier/Features/Load), so this is intentionally a separate surface
// rather than feeding into the legacy list.
public interface IFreeServersProvider
{
    Task<ApiResponseResult<ServerListResponse>> GetFreeServersAsync(CancellationToken cancellationToken = default);
}
