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
using SyncVPN.Api.V2.Contracts.Servers;
using SyncVPN.Client.Logic.Purchases.Contracts;

namespace SyncVPN.Client.Logic.Purchases;

public class ServerFavoritesClient : IServerFavoritesClient
{
    private readonly ISyncVpnApiClient _apiClient;

    public ServerFavoritesClient(ISyncVpnApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponseResult<FavoriteServersResponse>> GetFavoritesAsync(CancellationToken cancellationToken = default)
    {
        return _apiClient.GetFavoriteServersAsync(cancellationToken);
    }

    public Task<ApiResponseResult<FavoriteServerActionResponse>> AddFavoriteAsync(long serverId, CancellationToken cancellationToken = default)
    {
        return _apiClient.AddFavoriteServerAsync(serverId, cancellationToken);
    }

    public Task<ApiResponseResult<RemoveFavoriteServerResponse>> RemoveFavoriteAsync(long serverId, CancellationToken cancellationToken = default)
    {
        return _apiClient.RemoveFavoriteServerAsync(serverId, cancellationToken);
    }

    public Task<ApiResponseResult<RateServerResponse>> RateServerAsync(long serverId, int rate, CancellationToken cancellationToken = default)
    {
        return _apiClient.RateServerAsync(serverId, rate, cancellationToken);
    }
}
