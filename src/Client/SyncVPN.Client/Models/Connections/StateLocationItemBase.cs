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
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.States;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Models.Connections;

public abstract class StateLocationItemBase : HostLocationItemBase<State>
{
    public State State { get; }

    public override string Header => Localizer.GetStateName(State.Name, State.CountryCode);

    public override string Description => Localizer.GetCountryName(State.CountryCode);

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_State_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_State_UnderMaintenance")
                : null;

    public override ILocationIntent LocationIntent { get; }

    public override VpnTriggerDimension VpnTriggerDimension => IsSearchItem
        ? VpnTriggerDimension.SearchState
        : VpnTriggerDimension.CountriesState;

    protected StateLocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainWindowOverlayActivator overlayActivator,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IConnectionGroupFactory connectionGroupFactory,
        ILocationItemFactory locationItemFactory,
        State state,
        bool showBaseLocation,
        bool isSearchItem)
        : base(localizer,
               serversLoader,
               connectionManager,
               overlayActivator,
               upsellCarouselWindowActivator,
               connectionGroupFactory,
               locationItemFactory,
               state,
               isSearchItem)
    {
        State = state;
        IsDescriptionVisible = showBaseLocation;

        LocationIntent = SingleStateLocationIntent.From(State.CountryCode, State.Name);
    }

    public void OnExpandState()
    {
        FetchSubItems();
    }

    public void OnCollapseState()
    {
        ClearSubItems();
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && !currentConnectionDetails.IsGateway
            && State.CountryCode == currentConnectionDetails.ExitCountryCode
            && State.Name == currentConnectionDetails.State
            && (FeatureIntent?.IsSupported(currentConnectionDetails.Server) ?? true);
    }
}