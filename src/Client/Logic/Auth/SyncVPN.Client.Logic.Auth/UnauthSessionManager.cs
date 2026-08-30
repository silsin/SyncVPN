/*
 * Copyright (c) 2025 Proton AG
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
using SyncVPN.Api.Contracts.Auth;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.ApiLogs;
using SyncVPN.Logging.Contracts.Events.UserLogs;

namespace SyncVPN.Client.Logic.Auth;

public class UnauthSessionManager : IUnauthSessionManager
{
    private readonly ILogger _logger;
    private readonly IApiClient _apiClient;
    private readonly ISettings _settings;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public UnauthSessionManager(ILogger logger, IApiClient apiClient, ISettings settings)
    {
        _logger = logger;
        _apiClient = apiClient;
        _settings = settings;
    }

    public async Task CreateIfDoesNotExistAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (IsUnauthSessionCreated())
            {
                return;
            }

            _logger.Info<UserLog>("Requesting unauth session to initiate login process.");

            ApiResponseResult<UnauthSessionResponse> unauthSessionResponse = await _apiClient.PostUnauthSessionAsync(cancellationToken);

            if (unauthSessionResponse.Success)
            {
                SaveUnauthSessionDetails(unauthSessionResponse.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.Error<ApiErrorLog>("An error occurred when requesting a new unauth session.", ex);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private bool IsUnauthSessionCreated()
    {
        return _settings.UnauthUniqueSessionId != null
            && _settings.UnauthAccessToken != null
            && _settings.UnauthRefreshToken != null;
    }

    private void SaveUnauthSessionDetails(UnauthSessionResponse response)
    {
        _settings.UnauthUniqueSessionId = response.UniqueSessionId;
        _settings.UnauthAccessToken = response.AccessToken;
        _settings.UnauthRefreshToken = response.RefreshToken;
    }

    public void Revoke()
    {
        _settings.UnauthUniqueSessionId = null;
        _settings.UnauthAccessToken = null;
        _settings.UnauthRefreshToken = null;
    }

    public async Task RecreateAsync(CancellationToken cancellationToken)
    {
        Revoke();
        await CreateIfDoesNotExistAsync(cancellationToken);
    }
}