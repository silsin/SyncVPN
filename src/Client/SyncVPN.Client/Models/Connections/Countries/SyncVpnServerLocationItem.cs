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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SyncVPN.Api.V2.Contracts.Account;
using SyncVPN.Api.V2.Contracts.Servers;
using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Contracts.Enums;
using SyncVPN.Client.Contracts.Messages;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.SyncVpnServers;
using SyncVPN.Client.Logic.Connection.RequestCreators;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Services.ServerPing;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Models.Connections.Countries;

// One row of the new SyncVPN backend's server catalog (GET /servers + GET /servers/pro, merged - see
// IFreeServersCache), rendered inline in the Countries sidebar list (see AllCountriesComponentViewModel)
// instead of the standalone flyout this used to live behind. Deliberately does not derive from
// ServerLocationItemBase - that type is built around the legacy backend Server model
// (Tier/Features/Load/physical-server list), which this catalog has no equivalent for; faking one
// would show made-up load/feature data. Connects via SyncVpnServerLocationIntent, which bypasses the
// legacy candidate-selection pipeline entirely.
//
// Unlike every other row in the app, clicking the row body does NOT connect - it only pans the map to
// this server (SelectCommand) via MapLocationSelectedMessage. Only the dedicated Connect button
// (ToggleConnectionCommand, inherited from ConnectionItemBase) actually starts a connection.
public partial class SyncVpnServerLocationItem : LocationItemBase
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IServerPingService _serverPingService;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    public ServerListItem Server { get; }

    public override string Header { get; }

    public string CountryCode => Server.Country.ShortName;

    public override string Description { get; }

    public override string? ToolTip => Server.Country.Name;

    // The protocol this row will actually connect with: the user's global Settings preference if this
    // server supports it, otherwise the same WireGuard-if-present-else-first fallback used elsewhere -
    // see SyncVpnAccountClaimMapper.ResolveServerProtocol. Computed once at construction time; a
    // change to the global protocol setting while this row is already on screen doesn't retroactively
    // relabel it (matches how every other row's connect behavior is snapshotted at list-build time).
    public string ProtocolLabel { get; }

    // Real measured round-trip time to this server, fetched lazily (see InvalidatePingAsync) since the
    // public catalog carries no IP to ping until the server is claimed - see IServerPingService. Empty
    // until measured (or if the server couldn't be reached) rather than showing a made-up number.
    [ObservableProperty]
    private string _pingText = string.Empty;

    public bool IsPro => Server.Free == 0;

    public override ConnectionGroupType GroupType => IsPro ? ConnectionGroupType.PremiumLocations : ConnectionGroupType.FreeServers;

    public override ILocationIntent LocationIntent { get; }

    public override IFeatureIntent? FeatureIntent { get; } = null;

    public override VpnTriggerDimension VpnTriggerDimension => VpnTriggerDimension.CountriesServer;

    protected override string AutomationName => $"SyncVpn_Server_{Server.Id}";

    public SyncVpnServerLocationItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IEventMessageSender eventMessageSender,
        IServerPingService serverPingService,
        IUIThreadDispatcher uiThreadDispatcher,
        ISettings settings,
        ServerListItem server)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator,
               isSearchItem: false)
    {
        _eventMessageSender = eventMessageSender;
        _serverPingService = serverPingService;
        _uiThreadDispatcher = uiThreadDispatcher;
        Server = server;
        Header = server.Name;
        Description = server.City is not null ? $"{server.City}, {server.Country.Name}" : server.Country.Name;
        IsDescriptionVisible = !string.IsNullOrEmpty(Description);

        (string protocol, string? transport) = SyncVpnAccountClaimMapper.ResolveServerProtocolAndTransport(settings.VpnProtocol, server.Protocols);
        ProtocolLabel = GetProtocolLabel(protocol);

        LocationIntent = new SyncVpnServerLocationIntent(server.Id, server.Name, protocol, transport, isForPaidUsersOnly: server.Free == 0);

        _ = InvalidatePingAsync(protocol);
    }

    // Fire-and-forget by design: this row must render immediately with PingText blank, not block
    // construction on a network round trip. IServerPingService caches by ServerId, so this is at most
    // one claim+ping per server for the whole process lifetime, however many times a row is rebuilt.
    private async Task InvalidatePingAsync(string protocol)
    {
        int? pingMs = await _serverPingService.GetPingMsAsync(Server.Id, protocol, transport: null);
        _uiThreadDispatcher.TryEnqueue(() => PingText = pingMs.HasValue ? $"{pingMs} ms" : string.Empty);
    }

    private string GetProtocolLabel(string wireProtocol)
    {
        string resourceKey = wireProtocol switch
        {
            SyncVpnProtocols.WireGuard => "Settings_Protocols_WireGuardUdp_Title",
            SyncVpnProtocols.OpenVpn => "Settings_Protocols_OpenVpnTcp_Title",
            SyncVpnProtocols.L2tp => "Settings_Protocols_L2tp_Title",
            SyncVpnProtocols.Sstp => "Settings_Protocols_Sstp_Title",
            _ => string.Empty,
        };

        return string.IsNullOrEmpty(resourceKey) ? wireProtocol : Localizer.Get(resourceKey);
    }

    // Mirrors ConnectionItemBase's default (IsRestricted = !isPaidUser), but scoped to whether THIS
    // server actually requires a paid plan - a free server must never be restricted regardless of the
    // caller's plan, and a Pro server must always be restricted for a non-paid caller.
    public override void InvalidateIsRestricted(bool isPaidUser)
    {
        IsRestricted = LocationIntent.IsForPaidUsersOnly && !isPaidUser;
    }

    // Bound to the row body's click - pans the map to this server without connecting. Pressing the
    // dedicated Connect button is the only way to actually start a connection (see ToggleConnectionAsync).
    [RelayCommand]
    private void Select()
    {
        if (Server.Location != null)
        {
            _eventMessageSender.Send(new MapLocationSelectedMessage
            {
                Latitude = Server.Location.Lat,
                Longitude = Server.Location.Long,
                CountryCode = CountryCode,
                IsExplicitSelection = true
            });
        }
    }

    // Also pans the map to this exact server ahead of the connect attempt resolving -
    // MapComponentViewModel's usual ConnectionStatusChangedMessage-driven panning only fires once the
    // native service reports a live connection, which is too late for immediate feedback here.
    protected override Task ToggleConnectionAsync()
    {
        if (!IsActiveConnection)
        {
            Select();
        }

        return base.ToggleConnectionAsync();
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails is not null
            && currentConnectionDetails.OriginalConnectionIntent.Location.IsSameAs(LocationIntent);
    }
}
