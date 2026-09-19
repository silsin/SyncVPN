/*
 * Copyright (c) 2025 Proton AG
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

using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Models;

namespace SyncVPN.Client.Logic.Connection.Contracts.SerializableEntities.Intents;

public class SerializableLocationIntent
{
    public string? TypeName { get; set; }

    public string? CountryCode { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    [Obsolete("Use Server or GatewayServer instead")]
    public string? Id { get; set; }

    [Obsolete("Use Server or GatewayServer instead")]
    public string? Name { get; set; }

    public string? GatewayName { get; set; }

    [Obsolete("Use Strategy instead")]
    public int? FreeServerType { get; set; }

    [Obsolete("Use ServerToExclude instead")]
    public string? FreeServerExcludedLogicalId { get; set; }

    [Obsolete("Use Strategy instead")]
    public string? Kind { get; set; }

    public bool? IsToExcludeMyCountry { get; set; }

    public List<string>? CountryCodes { get; set; }

    public List<string>? States { get; set; }

    public List<string>? Cities { get; set; }

    public ServerInfo? Server { get; set; }

    public List<ServerInfo>? Servers { get; set; }

    public List<string>? GatewayNames { get; set; }

    public GatewayServerInfo? GatewayServer { get; set; }

    public List<GatewayServerInfo>? GatewayServers { get; set; }

    public SelectionStrategy? Strategy { get; set; }

    public ServerInfo? ServerToExclude { get; set; }

    // SyncVpnServerLocationIntent-only: the new SyncVPN backend's per-server intent has no legacy
    // equivalent to reuse (Server above only carries Id/Name), so these round-trip its remaining fields.
    public string? Protocol { get; set; }

    public string? Transport { get; set; }

    public bool? IsForPaidUsersOnly { get; set; }
}