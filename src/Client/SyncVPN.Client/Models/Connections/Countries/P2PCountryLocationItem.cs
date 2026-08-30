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

using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Enums;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Contracts.Enums;

namespace SyncVPN.Client.Models.Connections.Countries;

public class P2PCountryLocationItem : CountryLocationItemBase
{
    public override ConnectionGroupType GroupType { get; } = ConnectionGroupType.P2PCountries;

    public override IFeatureIntent? FeatureIntent { get; } = new P2PFeatureIntent();

    protected override bool IsSubGroupHeaderHidden => true;

    public P2PCountryLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainWindowOverlayActivator overlayActivator,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IConnectionGroupFactory connectionGroupFactory,
        ILocationItemFactory locationItemFactory,
        Country country,
        bool isSearchItem)
        : base(localizer,
               serversLoader,
               connectionManager,
               overlayActivator,
               upsellCarouselWindowActivator,
               connectionGroupFactory,
               locationItemFactory,
               country,
               isSearchItem)
    {
        IsUnderMaintenance = country.IsP2PUnderMaintenance;
    }

    protected override IEnumerable<ConnectionItemBase> GetSubItems()
    {
        IEnumerable<ConnectionItemBase> states =
            ServersLoader.GetStatesByFeaturesAndCountryCode(ServerFeatures.P2P, ExitCountryCode)
                         .Select(state => LocationItemFactory.GetP2PState(state, showBaseLocation: false, isSearchItem: IsSearchItem));

        return states.Any()
            ? states
            : ServersLoader.GetCitiesByFeaturesAndCountryCode(ServerFeatures.P2P, ExitCountryCode)
                           .Select(city => LocationItemFactory.GetP2PCity(city, showBaseLocation: false, isSearchItem: IsSearchItem));
    }
}