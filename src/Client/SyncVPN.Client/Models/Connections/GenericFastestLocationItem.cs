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
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Models.Connections;

public class GenericFastestLocationItem : LocationItemBase
{
    public override ILocationIntent LocationIntent { get; }

    public override IFeatureIntent? FeatureIntent => null;

    public override ConnectionGroupType GroupType { get; }

    public override string Header => Localizer.Get("Connections_Fastest");

    public override string? ToolTip => null;

    public override VpnTriggerDimension VpnTriggerDimension { get; } = VpnTriggerDimension.Profile;

    public GenericFastestLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        ConnectionGroupType groupType,
        ILocationIntent locationIntent)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator,
               false)
    {
        GroupType = groupType;
        LocationIntent = locationIntent;
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return false;
    }
}