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

using SyncVPN.Builds.Variables;
using SyncVPN.Common.Core.OperatingSystems.EnvironmentVariables;
using SyncVPN.Configurations.Contracts;

namespace SyncVPN.Api.V2;

public interface ISyncVpnAppTokenProvider
{
    // The app-level "token" header (AppToken) required on every call to the new SyncVPN API.
    // Never hardcode a real value here - it's a secret, injected at build time the same way
    // as the other entries in GlobalConfig (e.g. SentryDsn), with an env var override for local dev.
    string GetAppToken();
}

public class SyncVpnAppTokenProvider : ISyncVpnAppTokenProvider
{
    private readonly IConfiguration _configuration;

    public SyncVpnAppTokenProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Precedence: env var (CI/automation) > _configuration.json's SyncVpnAppToken (local dev override -
    // takes effect on every app start, no process/IDE restart needed, unlike an env var) > GlobalConfig
    // (build-injected, real releases).
    public string GetAppToken()
    {
        return EnvironmentVariableLoader.GetOrNull("SYNCVPN_APP_TOKEN")
            ?? (string.IsNullOrEmpty(_configuration.SyncVpnAppToken) ? null : _configuration.SyncVpnAppToken)
            ?? GlobalConfig.SyncVpnAppToken;
    }
}
