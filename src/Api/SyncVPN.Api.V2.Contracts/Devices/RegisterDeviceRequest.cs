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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace SyncVPN.Api.V2.Contracts.Devices;

public class RegisterDeviceRequest
{
    [JsonProperty("device_id")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonProperty("platform")]
    public DevicePlatform Platform { get; set; } = DevicePlatform.Windows;

    // Required by the backend (verified live: omitting it fails with 422 "Language is required to
    // register a new device."). Accepts either a bare code ("en") or a culture-qualified one ("en-US").
    [JsonProperty("language")]
    public string Language { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("manufacturer")]
    public string? Manufacturer { get; set; }

    [JsonProperty("model")]
    public string? Model { get; set; }

    [JsonProperty("app_version")]
    public string? AppVersion { get; set; }

    [JsonProperty("app_build")]
    public string? AppBuild { get; set; }

    [JsonProperty("architecture")]
    public string? Architecture { get; set; }

    [JsonProperty("os_version")]
    public string? OsVersion { get; set; }

    [JsonProperty("locale")]
    public string? Locale { get; set; }

    [JsonProperty("timezone")]
    public string? Timezone { get; set; }

    // Windows has no FCM/APNs push integration, so there's no real push token to send. Per direction,
    // this carries a locally-generated, per-installation GUID instead (persisted in
    // ISettings.SyncVpnPushToken) so the backend still gets a stable, unique value rather than null.
    [JsonProperty("push_token")]
    public string? PushToken { get; set; }

    // Same reasoning as PushToken: no OneSignal SDK on Windows, so this is a locally-generated,
    // persisted GUID (ISettings.SyncVpnOneSignalSubscriptionId), not a real OneSignal subscription id.
    [JsonProperty("onesignal_subscription_id")]
    public string? OneSignalSubscriptionId { get; set; }

    [JsonProperty("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}
