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

using SyncVPN.Api.Contracts;
using SyncVPN.Api.Contracts.Servers;
using SyncVPN.Client.Logic.Servers.Contracts.Models;

namespace SyncVPN.Client.Logic.Servers.Cache;

public interface IServersCache
{
    IReadOnlyList<Server> Servers { get; }
    IReadOnlyList<FreeCountry> FreeCountries { get; }
    IReadOnlyList<Country> Countries { get; }
    IReadOnlyList<State> States { get; }
    IReadOnlyList<City> Cities { get; }
    IReadOnlyList<Gateway> Gateways { get; }
    IReadOnlyList<SecureCoreCountryPair> SecureCoreCountryPairs { get; }

    bool IsEmpty(); 
    bool AreAllServersUnderMaintenance();
    bool IsStale();
    bool IsOutdated();
    bool IsLoadOutdated();
    bool HasServersRequestFailed();
    bool HasNoServers();

    void Clear();
    void LoadFromFileIfEmpty();
    void ReprocessServers();

    Task UpdateAsync(CancellationToken cancellationToken);
    Task UpdateLoadsAsync(CancellationToken cancellationToken);
    Task<ApiResponseResult<LookupServerResponse>?> LookupAsync(string input);
}