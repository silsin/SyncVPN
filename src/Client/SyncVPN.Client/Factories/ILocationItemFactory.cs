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

using ServerListItem = SyncVPN.Api.V2.Contracts.Servers.ServerListItem;
using SyncVPN.Client.Contracts.Enums;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Models.Connections;
using SyncVPN.Client.Models.Connections.Countries;
using SyncVPN.Client.Models.Connections.Gateways;

namespace SyncVPN.Client.Factories;

public interface ILocationItemFactory
{
    GenericCountryLocationItem GetGenericCountry(CountriesConnectionType connectionType, SelectionStrategy intentKind, bool excludeMyCountry, bool isSearchItem = false);

    GenericFastestLocationItem GetGenericFastestLocation(ConnectionGroupType groupType, ILocationIntent locationIntent);

    CountryLocationItem GetCountry(Country country, bool isSearchItem = false);

    StateLocationItem GetState(State state, bool showBaseLocation = false, bool isSearchItem = false);

    CityLocationItem GetCity(City city, bool showBaseLocation = false, bool isSearchItem = false);

    ServerLocationItem GetServer(Server server, bool isSearchItem = false);

    SyncVpnServerLocationItem GetSyncVpnServer(ServerListItem server);

    SecureCoreCountryLocationItem GetSecureCoreCountry(Country country, bool isSearchItem = false);

    SecureCoreCountryPairLocationItem GetSecureCoreCountryPair(SecureCoreCountryPair countryPair, bool isSearchItem = false);

    P2PCountryLocationItem GetP2PCountry(Country country, bool isSearchItem = false);

    P2PStateLocationItem GetP2PState(State state, bool showBaseLocation = false, bool isSearchItem = false);

    P2PCityLocationItem GetP2PCity(City city, bool showBaseLocation = false, bool isSearchItem = false);

    P2PServerLocationItem GetP2PServer(Server server, bool isSearchItem = false);

    TorCountryLocationItem GetTorCountry(Country country, bool isSearchItem = false);

    TorServerLocationItem GetTorServer(Server server, bool isSearchItem = false);

    GenericGatewayLocationItem GetGenericGateway(SelectionStrategy intentKind);

    GatewayLocationItem GetGateway(Gateway gateway);

    GatewayServerLocationItem GetGatewayServer(Server server);
}