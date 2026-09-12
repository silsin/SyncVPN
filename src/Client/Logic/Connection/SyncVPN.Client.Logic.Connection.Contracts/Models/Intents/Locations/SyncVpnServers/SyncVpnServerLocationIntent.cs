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

using SyncVPN.Client.Logic.Servers.Contracts.Models;

namespace SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.SyncVpnServers;

// Targets one specific server from the new SyncVPN backend's catalog (GET /servers /servers/pro) by
// id, bypassing the legacy candidate-selection pipeline entirely - see ConnectionRequestCreator,
// which recognizes this intent and claims the account directly via POST /account instead of picking
// from ServersLoader's (legacy Proton) cached Server list. IsSupported/FilterServers always report
// "no match" against that legacy list - truthfully, since this server doesn't exist there at all.
public class SyncVpnServerLocationIntent : LocationIntentBase
{
    public long ServerId { get; }
    public string ServerName { get; }
    public string Protocol { get; }
    public string? Transport { get; }

    // False for free servers, true for Pro-only ones - defaults to false since the only two call
    // sites that predate this parameter (the free-server map pin and row) always target free servers.
    public override bool IsForPaidUsersOnly { get; }

    public SyncVpnServerLocationIntent(long serverId, string serverName, string protocol, string? transport = null, bool isForPaidUsersOnly = false)
    {
        ServerId = serverId;
        ServerName = serverName;
        Protocol = protocol;
        Transport = transport;
        IsForPaidUsersOnly = isForPaidUsersOnly;
    }

    public override bool IsSameAs(ILocationIntent? intent)
    {
        return intent is SyncVpnServerLocationIntent other && ServerId == other.ServerId;
    }

    public override bool IsSupported(Server server)
    {
        return false;
    }

    public override string ToString()
    {
        return $"SyncVPN server {ServerName} ({ServerId})";
    }
}
