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
using SyncVPN.Api.V2.Contracts.Devices;

namespace SyncVPN.Api.V2.Contracts.Auth;

public class LoginResponseData
{
    // The 80-character raw DeviceToken - store it securely (encrypted setting), never log it.
    [JsonProperty("token")]
    public string Token { get; set; } = string.Empty;

    [JsonProperty("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonProperty("user")]
    public User User { get; set; } = new();

    [JsonProperty("device")]
    public Device Device { get; set; } = new();

    [JsonProperty("devices")]
    public List<Device> Devices { get; set; } = [];
}

public class LoginResponse
{
    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("requires_login")]
    public bool RequiresLogin { get; set; }

    [JsonProperty("requires_two_factor")]
    public bool RequiresTwoFactor { get; set; }

    [JsonProperty("data")]
    public LoginResponseData Data { get; set; } = new();
}
