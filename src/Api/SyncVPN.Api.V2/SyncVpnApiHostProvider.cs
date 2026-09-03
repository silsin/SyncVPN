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

using System;
using SyncVPN.Configurations.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.ApiLogs;

namespace SyncVPN.Api.V2;

public interface ISyncVpnApiHostProvider
{
    Uri GetBaseUri();
}

// Unlike ApiHostProvider (Proton), the new backend has no alternative-routing/anti-censorship
// host swap - that mechanism stays Proton-only per the hybrid-backend decision.
public class SyncVpnApiHostProvider : ISyncVpnApiHostProvider
{
    // Matches DefaultUrlsConfigurationFactory - used only if IConfiguration.Urls.SyncVpnApiUrl is
    // missing/empty/invalid (e.g. a stale local config cache from before this field existed), so a
    // bad config value can never crash app startup. This provider is constructed eagerly (via
    // DeviceRegistrationObserver's AutoActivate registration) regardless of whether the new backend
    // is actually enabled, so it must never throw.
    private const string FallbackApiUrl = "https://syncvpn.com/api/";

    private readonly IConfiguration _config;
    private readonly ILogger _logger;

    public SyncVpnApiHostProvider(IConfiguration config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    public Uri GetBaseUri()
    {
        string configuredUrl = _config.Urls.SyncVpnApiUrl;

        // HttpClient.BaseAddress + a relative request path only appends correctly when the base ends
        // in '/': new Uri(new Uri("https://syncvpn.com/api"), "devices/register") resolves to
        // "https://syncvpn.com/devices/register" (the last segment gets replaced, not extended) per
        // standard relative-URI resolution. Every SyncVpnApiClient call is relative, so a missing
        // trailing slash here silently drops "/api" from every single request. Normalize here rather
        // than trusting every config source (defaults, _configuration.json, remote config) to remember it.
        if (Uri.TryCreate(EnsureTrailingSlash(configuredUrl), UriKind.Absolute, out Uri? uri))
        {
            return uri;
        }

        _logger.Error<ApiErrorLog>($"Invalid or missing SyncVpnApiUrl ('{configuredUrl}'), falling back to '{FallbackApiUrl}'.");
        return new Uri(FallbackApiUrl);
    }

    private static string EnsureTrailingSlash(string url)
    {
        return string.IsNullOrEmpty(url) || url.EndsWith('/') ? url : url + "/";
    }
}
