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

using System.Collections.Concurrent;
using SyncVPN.Client.Settings.Contracts.Enums;
using SyncVPN.Client.Settings.Contracts.Models;
using SyncVPN.Common.Core.Geographical;
using SyncVPN.Common.Core.Networking;
using SyncVPN.Dns.Contracts;

namespace SyncVPN.Client.Settings.Contracts;

public interface IGlobalSettings
{
    string Language { get; set; }
    DeviceLocation? DeviceLocation { get; set; }
    string? UserId { get; set; }
    string? AccessToken { get; set; }
    string? RefreshToken { get; set; }
    string? UniqueSessionId { get; set; }
    string? UnauthAccessToken { get; set; }
    string? UnauthRefreshToken { get; set; }
    string? UnauthUniqueSessionId { get; set; }
    bool IsAutoLaunchEnabled { get; set; }
    AutoLaunchMode AutoLaunchMode { get; set; }
    int[] WireGuardUdpPorts { get; set; }
    int[] WireGuardTcpPorts { get; set; }
    int[] WireGuardTlsPorts { get; set; }
    int[] OpenVpnTcpPorts { get; set; }
    int[] OpenVpnUdpPorts { get; set; }
    ConcurrentDictionary<string, DnsResponse>? DnsCache { get; set; } // VPNWIN-2098 - Move to its own file
    bool IsAlternativeRoutingEnabled { get; set; }
    bool IsKillSwitchEnabled { get; set; }
    bool IsBetaAccessEnabled { get; set; }
    bool AreAutomaticUpdatesEnabled { get; set; }
    bool IsGlobalSettingsMigrationDone { get; set; }
    KillSwitchMode KillSwitchMode { get; set; }
    List<FeatureFlag> FeatureFlags { get; set; }
    bool IsFeatureConnectedServerCheckEnabled { get; set; }
    TimeSpan ConnectedServerCheckInterval { get; set; }
    ChangeServerSettings ChangeServerSettings { get; set; }
    bool IsShareCrashReportsEnabled { get; set; }
    string? ActiveAlternativeApiBaseUrl { get; set; }
    VpnProtocol[] DisabledSmartProtocols { get; set; }
    int TotalCountryCount { get; set; }
    int TotalServerCount { get; set; }
    string? LastProcessVersionMismatchRestartVersions { get; set; }
    DateTimeOffset? LastProcessVersionMismatchRestartUtcDate { get; set; }
    TimeSpan WireGuardConnectionTimeout { get; set; }
    bool IsEfficiencyModeAllowed { get; set; }

    // Local override for the new-vs-legacy SyncVPN backend migration (see BackendModeProvider).
    // false (default) = defer to the remote feature flag; true = force the new backend on. Engineering/QA only.
    bool NewBackendOverride { get; set; }

    // Deviceid issued by the new SyncVPN backend's POST /devices/register. Null until that call has
    // succeeded at least once (only happens when NewBackendOverride/the remote flag enables it).
    string? SyncVpnDeviceId { get; set; }

    // Bearer DeviceToken from the new SyncVPN backend's login flow (Phase 4). Null until a device has
    // logged in - Pro-gated calls like PATCH /account/dns-filters require it and will 401 without it.
    string? SyncVpnDeviceToken { get; set; }

    Dictionary<string, Dictionary<string, string?>>? LegacySettingsByUsername { get; set; }
}