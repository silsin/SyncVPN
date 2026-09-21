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
using System.Linq;
using SyncVPN.Api.Contracts.Geographical;
using SyncVPN.Api.Contracts.Servers;
using SyncVPN.Api.V2.Contracts.Servers;

namespace SyncVPN.Api;

// Translates the new SyncVPN backend's server catalog (GET /servers) into the legacy backend-shaped
// ServersResponse/LogicalServerResponse DTOs, so the existing entity mapper and ServersCache pipeline
// (built for vpn/v2/logicals) can consume it unchanged. See the migration plan for the fields the new
// catalog genuinely has no equivalent for (load, score, Secure Core, per-server key material - the last
// of those only becomes available once a server+protocol is claimed via POST /account, not from this
// public catalog) - those are left at safe defaults rather than invented.
public static class SyncVpnServersResponseMapper
{
    // Placeholder StatusID: the new backend has no equivalent to the legacy backend's binary loads/status blob, so
    // there's nothing meaningful to key it by. ServersCache is told (via IBackendModeProvider) to skip
    // the binary-loads fetch entirely when this backend is active, so the value itself is never used
    // to make a request - it only needs to be non-null to satisfy ServersResponse.StatusId.
    private const string PlaceholderStatusId = "syncvpn-v2";

    public static ServersResponse Map(IEnumerable<ServerListItem> items)
    {
        return new ServersResponse
        {
            Servers = items.Select(MapServer).ToList(),
            ResponseMetadata = new LogicalsMetadataResponse { ListIsTruncated = false },
            StatusId = PlaceholderStatusId,
        };
    }

    private static LogicalServerResponse MapServer(ServerListItem item)
    {
        string countryCode = item.Country.ShortName.ToUpperInvariant();
        ServerLocationResponse location = new()
        {
            Latitude = (float)(item.Location?.Lat ?? 0),
            Longitude = (float)(item.Location?.Long ?? 0),
        };

        return new LogicalServerResponse
        {
            Id = item.Id.ToString(),
            Name = item.Name,
            City = item.City ?? string.Empty,
            State = string.Empty,
            EntryCountry = countryCode,
            ExitCountry = countryCode,
            Domain = item.Datacenter ?? string.Empty,
            Tier = (sbyte)(item.Free == 1 ? 0 : 2), // ServerTiers.Free / ServerTiers.Plus
            Features = 0, // No P2P/SecureCore/Tor/B2B signal in this catalog - treated as a standard server.
            Status = 1, // The catalog only lists servers currently offered; no maintenance flag to map.
            Load = 0, // Not provided by this endpoint.
            Score = 0,
            HostCountry = string.Empty, // Empty => not a virtual/Secure-Core-hosted location.
            GatewayName = string.Empty, // No B2B gateway concept here.
            Servers = MapPhysicalServers(item),
            StatusReference = new StatusReferenceResponse(),
            EntryLocation = location,
            ExitLocation = location,
        };
    }

    private static List<PhysicalServerResponse> MapPhysicalServers(ServerListItem item)
    {
        // No connection secrets are available from the catalog endpoint - those are only issued per
        // device+server+protocol by POST /account (Phase 5). This placeholder exists so the internal
        // Server model has a non-empty Servers list to display, not to be connected through directly.
        return
        [
            new PhysicalServerResponse
            {
                Id = item.Id.ToString(),
                EntryIp = string.Empty,
                Domain = item.Datacenter ?? string.Empty,
                Status = 1,
                Label = string.Empty,
                X25519PublicKey = string.Empty,
                Signature = string.Empty,
                EntryPerProtocol = new EntryPerProtocolResponse(),
            }
        ];
    }
}
