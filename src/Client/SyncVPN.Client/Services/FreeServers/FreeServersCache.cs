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
using SyncVPN.Api.V2.Contracts.Servers;

namespace SyncVPN.Client.Services.FreeServers;

public class FreeServersCache : IFreeServersCache
{
    private readonly object _lock = new();

    private IReadOnlyList<ServerListItem> _servers = [];

    public IReadOnlyList<ServerListItem> GetServers()
    {
        lock (_lock)
        {
            return _servers;
        }
    }

    public void SetServers(IReadOnlyList<ServerListItem> servers)
    {
        lock (_lock)
        {
            _servers = servers;
        }
    }

    public bool IsProtocolAvailable(string wireProtocol)
    {
        IReadOnlyList<ServerListItem> servers = GetServers();
        return servers.Count == 0 || servers.Any(s => s.Protocols.Contains(wireProtocol));
    }
}
