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

using SyncVPN.Api.V2.Contracts.Servers;

namespace SyncVPN.Client.Services.FreeServers;

// In-memory holder for the new SyncVPN backend's free-server catalog (GET /servers), populated by
// FreeServersObserver and read synchronously by the Countries sidebar list (AllCountriesComponentViewModel)
// the same way IServersLoader's cache is read - no per-call network hit.
public interface IFreeServersCache
{
    IReadOnlyList<ServerListItem> GetServers();

    void SetServers(IReadOnlyList<ServerListItem> servers);

    // Whether any cached server advertises this wire protocol string (see SyncVpnProtocols). Fails
    // open (returns true) while the cache is still empty - before the catalog has loaded for the
    // first time - so protocol pickers don't render as empty/broken on a cold start.
    bool IsProtocolAvailable(string wireProtocol);
}
