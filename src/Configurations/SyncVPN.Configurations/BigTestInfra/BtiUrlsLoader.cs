/*
 * Copyright (c) 2023 Proton AG
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

using SyncVPN.Builds.Variables;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Common.Core.OperatingSystems.EnvironmentVariables;
using SyncVPN.Configurations.Contracts.Entities;
using SyncVPN.Configurations.Entities;

namespace SyncVPN.Configurations.BigTestInfra;

public static class BtiUrlsLoader
{
    public static IUrlsConfiguration Get(object? defaultValue)
    {
        UrlsConfiguration config = defaultValue is not null && defaultValue is UrlsConfiguration uc ? uc : new();
        SetConfigIfNotNull(config);
        return config;
    }

    private static void SetConfigIfNotNull(UrlsConfiguration config)
    {
        Uri? apiUri = GetApiUri();
        if (apiUri is not null)
        {
            config.ApiUrl = apiUri.AbsoluteUri;
        }
    }

    public static Uri? GetApiUri()
    {
        string? apiUrl = GetApiUrl();
        return apiUrl is not null && apiUrl.IsHttpUri(out Uri uri) ? uri : null;
    }

    private static string? GetApiUrl()
    {
        string? apiUrl = EnvironmentVariableLoader.GetOrNull("BTI_API_DOMAIN");
        if (string.IsNullOrEmpty(apiUrl))
        {
            apiUrl = GlobalConfig.BtiApiDomain;
            if (!string.IsNullOrEmpty(apiUrl))
            {
                return apiUrl;
            }
        }
        else
        {
            return apiUrl;
        }
        return null;
    }
}