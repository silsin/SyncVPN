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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts.Account;
using SyncVPN.Api.V2.Contracts.Servers;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.SyncVpnServers;
using SyncVPN.Client.Logic.Purchases.Contracts;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.UI.Main.Home.Details.Flyouts;

// Renders GET /servers (the new SyncVPN backend's free-server catalog) directly - a standalone list,
// not fed into the legacy sidebar/search Server model, since the new API has no Tier/Features/Load
// equivalent to map onto that model (see IFreeServersProvider remarks). Clicking a row connects
// immediately, same as every other server/country row in this app (ConnectionItemBase.
// ToggleConnectionCommand) - there's no separate "select then confirm" step anywhere else in the UI,
// so this doesn't invent one. The Home screen's existing connection card/map is what then shows the
// connection progressing; this flyout has no Connect button of its own.
public partial class FreeServersFlyoutViewModel : ActivatableViewModelBase
{
    private readonly IFreeServersProvider _freeServersProvider;
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool _hasError;

    public ObservableCollection<FreeServerItem> Servers { get; } = [];

    public bool HasServers => Servers.Count > 0;
    public bool IsEmpty => !IsLoading && !HasError && !HasServers;

    public FreeServersFlyoutViewModel(
        IFreeServersProvider freeServersProvider,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _freeServersProvider = freeServersProvider;
        _connectionManager = connectionManager;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        LoadServersAsync().FireAndForget();
    }

    [RelayCommand]
    private async Task ConnectAsync(FreeServerItem? item)
    {
        if (item is null)
        {
            return;
        }

        IConnectionIntent intent = new ConnectionIntent(
            new SyncVpnServerLocationIntent(item.Id, item.Name, item.PreferredProtocol));

        await _connectionManager.ConnectAsync(VpnTriggerDimension.CountriesServer, intent);
    }

    private async Task LoadServersAsync()
    {
        IsLoading = true;
        HasError = false;

        ApiResponseResult<ServerListResponse> response = await _freeServersProvider.GetFreeServersAsync();

        if (!IsActive)
        {
            return;
        }

        IsLoading = false;

        if (!response.Success || response.Value is null)
        {
            HasError = true;
            return;
        }

        Servers.Clear();
        foreach (ServerListItem server in response.Value.Data)
        {
            Servers.Add(new FreeServerItem(server));
        }

        OnPropertyChanged(nameof(HasServers));
        OnPropertyChanged(nameof(IsEmpty));
    }
}

public class FreeServerItem
{
    private const string Unknown = "—";

    public long Id { get; }
    public string Name { get; }
    public string CountryName { get; }
    public string CountryEmoji { get; }
    public string City { get; }
    public string Datacenter { get; }
    public string CoordinatesText { get; }
    public string ProtocolsText { get; }

    // WireGuard preferred when available, else the first protocol the server advertises.
    public string PreferredProtocol { get; }

    public FreeServerItem(ServerListItem server)
    {
        Id = server.Id;
        Name = server.Name;
        CountryName = server.Country.Name;
        CountryEmoji = server.Country.Emoji ?? string.Empty;
        City = server.City ?? Unknown;
        Datacenter = server.Datacenter ?? Unknown;
        CoordinatesText = server.Location is { } location
            ? $"{location.Lat:F4}, {location.Long:F4}"
            : Unknown;
        ProtocolsText = string.Join(" · ", server.Protocols);
        PreferredProtocol = server.Protocols.Contains(SyncVpnProtocols.WireGuard)
            ? SyncVpnProtocols.WireGuard
            : server.Protocols.FirstOrDefault() ?? SyncVpnProtocols.WireGuard;
    }
}
