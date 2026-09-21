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

// GET /servers and GET /servers/pro - the new backend's server catalog. Distinct from IServersLoader
// (SyncVPN.Client.Logic.Servers.Contracts), which owns the legacy backend Physical/Logical server model -
// the two models aren't compatible (this one has no Tier/Features/Load). Rows built from this are
// rendered directly inside the Countries sidebar list (see SyncVpnServerLocationItem) rather than
// mapped onto the legacy Server model.
public interface IFreeServersProvider
{
    Task<ApiResponseResult<ServerListResponse>> GetFreeServersAsync(CancellationToken cancellationToken = default);

    Task<ApiResponseResult<ServerListResponse>> GetProServersAsync(CancellationToken cancellationToken = default);
}
