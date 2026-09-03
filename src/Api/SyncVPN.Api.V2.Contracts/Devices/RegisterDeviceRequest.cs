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

    [JsonProperty("app_version")]
    public string? AppVersion { get; set; }

    [JsonProperty("os_version")]
    public string? OsVersion { get; set; }

    [JsonProperty("locale")]
    public string? Locale { get; set; }

    [JsonProperty("timezone")]
    public string? Timezone { get; set; }
}
