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
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Contracts.Enums;

namespace SyncVPN.Client.Models.Connections.Countries;

public class CityLocationItem : CityLocationItemBase
{
    public override ConnectionGroupType GroupType { get; } = ConnectionGroupType.Cities;

    public override IFeatureIntent? FeatureIntent { get; } = null;

    public CityLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainWindowOverlayActivator overlayActivator,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IConnectionGroupFactory connectionGroupFactory,
        ILocationItemFactory locationItemFactory,
        City city,
        bool showBaseLocation,
        bool isSearchItem)
        : base(localizer,
               serversLoader,
               connectionManager,
               overlayActivator,
               upsellCarouselWindowActivator,
               connectionGroupFactory,
               locationItemFactory,
               city,
               showBaseLocation,
               isSearchItem)
    {
        IsUnderMaintenance = city.IsStandardUnderMaintenance;
    }

    protected override IEnumerable<ConnectionItemBase> GetSubItems()
    {
        return ServersLoader.GetServersByCity(City)
                            .Select(s => LocationItemFactory.GetServer(s, isSearchItem: IsSearchItem));
    }
}