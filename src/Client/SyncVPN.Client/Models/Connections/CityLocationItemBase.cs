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

using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Cities;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Models.Connections;

public abstract class CityLocationItemBase : HostLocationItemBase<City>
{
    public City City { get; }

    public override string Header => Localizer.GetCityName(City.Name, City.CountryCode);

    public override string Description =>
        string.IsNullOrEmpty(City.StateName)
            ? Localizer.GetCountryName(City.CountryCode)
            : $"{City.StateName}, {Localizer.GetCountryName(City.CountryCode)}";

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_City_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_City_UnderMaintenance")
                : null;

    public override ILocationIntent LocationIntent { get; }

    public override VpnTriggerDimension VpnTriggerDimension => IsSearchItem
        ? VpnTriggerDimension.SearchCity
        : VpnTriggerDimension.CountriesCity;

    protected CityLocationItemBase(
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
               isSearchItem)
    {
        City = city;
        IsDescriptionVisible = showBaseLocation;

        LocationIntent = SingleCityLocationIntent.From(City.CountryCode, City.StateName, City.Name);
    }

    public void OnExpandCity()
    {
        FetchSubItems();
    }

    public void OnCollapseCity()
    {
        ClearSubItems();
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && !currentConnectionDetails.IsGateway
            && City.CountryCode == currentConnectionDetails.ExitCountryCode
            && City.StateName == currentConnectionDetails.State
            && City.Name == currentConnectionDetails.City
            && (FeatureIntent?.IsSupported(currentConnectionDetails.Server) ?? true);
    }
}