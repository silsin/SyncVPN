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

using SyncVPN.Client.Common.Enums;
using SyncVPN.Client.Contracts.Enums;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Models.Connections.Gateways;

public class GenericGatewayLocationItem : LocationItemBase
{
    public SelectionStrategy Strategy { get; }

    public bool ExcludeMyCountry { get; }

    public FlagType FlagType => Strategy switch
    {
        SelectionStrategy.Random => FlagType.Random,
        _ => FlagType.Fastest,
    };

    public override string Header => Localizer.GetGatewayName(string.Empty, Strategy);

    public override bool IsCounted => false;

    public override object FirstSortProperty => string.Empty;

    public override object SecondSortProperty => string.Empty;

    public override ILocationIntent LocationIntent { get; }

    public override IFeatureIntent? FeatureIntent { get; } = new B2BFeatureIntent();

    public override ConnectionGroupType GroupType => ConnectionGroupType.Gateways;

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_Gateway_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_Gateway_UnderMaintenance")
                : null;

    protected override string AutomationName => Strategy switch
    {
        SelectionStrategy.Fastest => "Fastest",
        SelectionStrategy.Random => "Random",
        _ => throw new NotImplementedException($"Intent kind '{Strategy}' is not supported."),
    };

    public override VpnTriggerDimension VpnTriggerDimension { get; } = VpnTriggerDimension.GatewaysGateway;

    public GenericGatewayLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        SelectionStrategy intentKind)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator, 
               false)
    {
        Strategy = intentKind;

        LocationIntent = MultiGatewayLocationIntent.From(Strategy);
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && currentConnectionDetails.OriginalConnectionIntent.Location.IsSameAs(LocationIntent)
            && FeatureIntent!.IsSupported(currentConnectionDetails.Server);
    }
}