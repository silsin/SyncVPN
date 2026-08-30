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

using SyncVPN.Client.Contracts.Enums;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Gateways;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Models.Connections.Gateways;

public class GatewayLocationItem : HostLocationItemBase<Gateway>
{
    public Gateway Gateway { get; }

    public override ConnectionGroupType GroupType => ConnectionGroupType.Gateways;

    public override string Header => Gateway.Name;

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_Gateway_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_Gateway_UnderMaintenance")
                : null;

    public override ILocationIntent LocationIntent { get; }

    public override IFeatureIntent? FeatureIntent { get; } = new B2BFeatureIntent();

    public override VpnTriggerDimension VpnTriggerDimension { get; } = VpnTriggerDimension.GatewaysGateway;

    public GatewayLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IMainWindowOverlayActivator overlayActivator,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IConnectionGroupFactory connectionGroupFactory,
        ILocationItemFactory locationItemFactory,
        Gateway gateway)
        : base(localizer,
               serversLoader,
               connectionManager,
               overlayActivator,
               upsellCarouselWindowActivator,
               connectionGroupFactory,
               locationItemFactory,
               gateway,
               false)
    {
        Gateway = gateway;

        LocationIntent = SingleGatewayLocationIntent.From(gateway.Name);
    }

    public void OnExpandGateway()
    {
        FetchSubItems();
    }

    public void OnCollapseGateway()
    {
        ClearSubItems();
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && currentConnectionDetails.IsGateway
            && Gateway.Name == currentConnectionDetails.GatewayName
            && (FeatureIntent?.IsSupported(currentConnectionDetails.Server) ?? true);
    }

    protected override IEnumerable<ConnectionItemBase> GetSubItems()
    {
        return ServersLoader.GetServersByGatewayName(Gateway.Name)
                            .Select(LocationItemFactory.GetGatewayServer);
    }
}