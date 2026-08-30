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

using Newtonsoft.Json;

namespace SyncVPN.Api.Contracts.Auth;

public class AuthRequest
{
    public string ClientEphemeral { get; set; }

    public string ClientProof { get; set; }

    [JsonProperty(PropertyName = "SRPSession")]
    public string SrpSession { get; set; }

    public string Username { get; set; }

    [JsonProperty(PropertyName = "SSOResponseToken")]
    public string SsoResponseToken { get; set; }
}