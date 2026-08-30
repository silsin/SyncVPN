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

using SyncVPN.Client.Common.Extensions;
using SyncVPN.Client.Contracts.Enums;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.GatewayServers;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Servers;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Enums;
using SyncVPN.Client.Logic.Servers.Contracts.Extensions;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Models.Connections;

public abstract class ServerLocationItemBase : LocationItemBase<Server>
{
    public Server Server { get; }

    public override string Header { get; }

    public string ServerTag { get; }

    public int ServerNumber { get; }

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Connections_Server_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Connections_Server_UnderMaintenance")
                : null;

    public double Load => Server.Load / 100d;

    public override object FirstSortProperty => IsUnderMaintenance;

    public override object SecondSortProperty => Load;

    // Show city as base location when the server belongs to a state
    public string BaseLocation => string.IsNullOrEmpty(Server.State)
        ? string.Empty
        : Localizer.GetCityName(Server.City, Server.ExitCountry);

    public bool IsVirtual => Server.IsVirtual;

    public bool IsFree => Server.Tier == ServerTiers.Free;

    public bool SupportsP2P => Server.Features.IsSupported(ServerFeatures.P2P);

    public bool SupportsTor => Server.Features.IsSupported(ServerFeatures.Tor);

    public override ILocationIntent LocationIntent { get; }

    public override VpnTriggerDimension VpnTriggerDimension => IsSearchItem
        ? VpnTriggerDimension.SearchServer
        : VpnTriggerDimension.CountriesServer;

    protected override string AutomationName => "Specific_Server";

    protected ServerLocationItemBase(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        Server server,
        bool isSearchItem)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator,
               server,
               isSearchItem)
    {
        Server = server;
        Header = server.Name;
        ServerTag = server.Name.GetServerTag();
        ServerNumber = server.Name.GetServerNumber();

        LocationIntent = string.IsNullOrEmpty(Server.GatewayName)
            ? SingleServerLocationIntent.From(Server.ExitCountry, Server.State, Server.City, ServerInfo.From(Server.Id, Server.Name))
            : SingleGatewayServerLocationIntent.From(Server.GatewayName, GatewayServerInfo.From(Server.Id, Server.Name, Server.ExitCountry));
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && Server.Id == currentConnectionDetails.ServerId;
    }
}