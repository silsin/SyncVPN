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
using SyncVPN.Api.V2.Contracts.Devices;

namespace SyncVPN.Api.V2.Contracts.Auth;

// GET /auth/me - validates the current DeviceToken. Note: the API accepts a DeviceToken for up to
// three months of inactivity before requiring full re-login - there is no refresh-token endpoint on
// this backend at all, unlike Proton's auth/refresh.
public class AuthenticatedDeviceResponseData
{
    [JsonProperty("user")]
    public User User { get; set; } = new();

    [JsonProperty("device")]
    public Device Device { get; set; } = new();
}

public class AuthenticatedDeviceResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("data")]
    public AuthenticatedDeviceResponseData Data { get; set; } = new();
}
