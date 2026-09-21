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

using System.Linq;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Models;
using SyncVPN.Common.Core.OperatingSystems.EnvironmentVariables;

namespace SyncVPN.Api.BackendSelection;

// Precedence, highest wins:
//   1. Env var SYNCVPN_USE_NEW_BACKEND_<CAPABILITY> (e.g. SYNCVPN_USE_NEW_BACKEND_SERVERS) - for CI/automation.
//   2. ISettings.NewBackendOverride - local engineering/QA toggle (currently only a master on/off; see DebugTools).
//   3. Remote feature flag "NewBackend<Capability>", fetched from the legacy feature-flags endpoint
//      (kept on the legacy backend per the hybrid-backend decision) - this is how staged/cohort rollout works.
//   4. Default: false for every capability except DeviceRegistration, which defaults to true - it's a
//      prerequisite for the new backend's guest/free-tier access model, has no legacy-backend equivalent
//      to preserve, and is inert on its own (registering a device makes no user-visible change and
//      breaks nothing), unlike Servers/DnsFilters/Auth/VpnProvisioning which do risk regressing an
//      existing, working legacy flow.
public class BackendModeProvider : IBackendModeProvider
{
    private const string EnvVarPrefix = "SYNCVPN_USE_NEW_BACKEND_";

    private readonly ISettings _settings;

    public BackendModeProvider(ISettings settings)
    {
        _settings = settings;
    }

    public bool IsNewBackendEnabled(BackendCapability capability)
    {
        bool? envOverride = GetEnvVarOverride(capability);
        if (envOverride.HasValue)
        {
            return envOverride.Value;
        }

        if (_settings.NewBackendOverride)
        {
            return true;
        }

        bool? remoteFlag = GetRemoteFeatureFlag(capability);
        if (remoteFlag.HasValue)
        {
            return remoteFlag.Value;
        }

        return capability == BackendCapability.DeviceRegistration;
    }

    private static bool? GetEnvVarOverride(BackendCapability capability)
    {
        string? value = EnvironmentVariableLoader.GetOrNull(EnvVarPrefix + capability.ToString().ToUpperInvariant());
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return value == "1" || value.Equals("true", System.StringComparison.OrdinalIgnoreCase);
    }

    private bool? GetRemoteFeatureFlag(BackendCapability capability)
    {
        string flagName = $"NewBackend{capability}";
        FeatureFlag flag = _settings.FeatureFlags.FirstOrDefault(f => f.Name == flagName);

        return flag.Name == flagName ? flag.IsEnabled : null;
    }
}
