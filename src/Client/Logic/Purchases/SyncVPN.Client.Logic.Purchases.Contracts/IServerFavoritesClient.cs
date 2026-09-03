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

// Server-side, per-device favorites (GET/POST/DELETE /servers/{id}/favorite) and ratings
// (POST /servers/{id}/rate) against the new backend. Distinct from the pre-existing, purely local
// IFavoriteServersStorage in SyncVPN.Client.Logic.Servers.Contracts, which is tied to the legacy
// Physical/Logical server model - the two are not interchangeable.
public interface IServerFavoritesClient
{
    Task<ApiResponseResult<FavoriteServersResponse>> GetFavoritesAsync(CancellationToken cancellationToken = default);

    Task<ApiResponseResult<FavoriteServerActionResponse>> AddFavoriteAsync(long serverId, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<RemoveFavoriteServerResponse>> RemoveFavoriteAsync(long serverId, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<RateServerResponse>> RateServerAsync(long serverId, int rate, CancellationToken cancellationToken = default);
}
