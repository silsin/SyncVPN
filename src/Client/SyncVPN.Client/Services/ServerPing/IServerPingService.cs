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

namespace SyncVPN.Client.Services.ServerPing;

public interface IServerPingService
{
    // Returns the real measured round-trip time in ms to the given SyncVPN backend server, or null if
    // it couldn't be claimed/reached. Cached per ServerId for the lifetime of the process - repeated
    // calls (e.g. the list rebuilding) never re-measure or re-claim the same server twice.
    Task<int?> GetPingMsAsync(long serverId, string protocol, string? transport, CancellationToken cancellationToken = default);
}
