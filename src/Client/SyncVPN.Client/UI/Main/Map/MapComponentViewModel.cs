/*
 * Copyright (c) 2024 Proton AG
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
using SyncVPN.Client.Common.UI.Controls.Map;
using SyncVPN.Client.Contracts.Messages;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.SyncVpnServers;
using SyncVPN.Client.Logic.Connection.RequestCreators;
using SyncVPN.Client.Logic.Servers;
using SyncVPN.Client.Logic.Servers.Cache;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Services.FreeServers;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.UI.Main.Map;

public partial class MapComponentViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<MapLocationSelectedMessage>
{
    private readonly ISettings _settings;
    private readonly IServersCache _serversCache;
    private readonly IFreeServersCache _freeServersCache;
    private readonly IConnectionManager _connectionManager;
    private readonly ICoordinatesProvider _coordinatesProvider;
    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;

    [ObservableProperty]
    private bool _isMainWindowVisible;

    [ObservableProperty]
    private List<Country> _countries = [];

    [ObservableProperty]
    private Country? _currentCountry;

    // Set by MapControl when the user taps a pin (two-way bound - see MapComponentView.xaml); the
    // screen's own Connect button (not a per-pin one) acts on this instead of connecting on tap.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedCountry))]
    [NotifyPropertyChangedFor(nameof(ConnectButtonText))]
    private Country? _selectedCountry;

    public bool HasSelectedCountry => SelectedCountry is not null;

    public string ConnectButtonText => SelectedCountry is not null
        ? string.Format(Localizer.Get("Map_ConnectTo"), SelectedCountry.Name)
        : string.Empty;

    public bool IsConnecting => _connectionManager.IsConnecting;
    public bool IsConnected => _connectionManager.IsConnected;
    public bool IsDisconnected => _connectionManager.IsDisconnected;

    public MapComponentViewModel(
        ISettings settings,
        IServersCache serversCache,
        IFreeServersCache freeServersCache,
        IConnectionManager connectionManager,
        ICoordinatesProvider coordinatesProvider,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _settings = settings;
        _serversCache = serversCache;
        _freeServersCache = freeServersCache;
        _connectionManager = connectionManager;
        _coordinatesProvider = coordinatesProvider;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;

        InvalidateActiveCountry();
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            InvalidateActiveCountry();

            // A pending selection (map pin tap, or a Free servers list row - see the other Receive
            // overload below) is only meant to drive the screen-level Connect button up until a
            // connection attempt actually starts - once it has (whether for this selection or another
            // path entirely, e.g. the list row's own inline Connect button), showing "Connect to X" here
            // would be stale.
            if (message.ConnectionStatus != ConnectionStatus.Disconnected)
            {
                SelectedCountry = null;
            }

            OnPropertyChanged(nameof(IsConnecting));
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(IsDisconnected));
        });
    }

    public void Receive(SettingChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            if (message.PropertyName == nameof(ISettings.DeviceLocation))
            {
                InvalidateActiveCountry();
            }
        });
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        ExecuteOnUIThread(() => IsMainWindowVisible = message.IsMainWindowVisible);
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            InvalidateCountries();
            InvalidateActiveCountry();
        });
    }

    // Fired the instant a free-server row is clicked (see SyncVpnServerLocationItem), well before
    // any live ConnectionStatusChangedMessage could arrive - lets the map pan immediately on click instead
    // of waiting on the native service to report a connection.
    public void Receive(MapLocationSelectedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            CurrentCountry = new Country
            {
                Code = message.CountryCode,
                Latitude = message.Latitude,
                Longitude = message.Longitude,
                IsUnderMaintenance = false
            };

            // A row click (as opposed to this message being sent to just pan the map to an
            // already-auto-picked "fastest server") is a deliberate selection - surface it through the
            // same SelectedCountry the map's own pin taps use, so the screen-level Connect button shows
            // it too. Looked up from Countries (built in InvalidateCountries) rather than using
            // CurrentCountry above directly, since only that entry carries the free-server metadata
            // (FreeServerId/Name/Protocol) ConnectAsync below needs to connect to this exact server.
            if (message.IsExplicitSelection)
            {
                SelectedCountry = Countries.FirstOrDefault(c =>
                    c.IsFreeServer && c.Latitude == message.Latitude && c.Longitude == message.Longitude)
                    ?? CurrentCountry;
            }
        });
    }

    protected override void OnLanguageChanged()
    {
        ExecuteOnUIThread(() =>
        {
            InvalidateCountries();
            InvalidateActiveCountry();
        });
    }

    private void InvalidateActiveCountry()
    {
        switch (_connectionManager.ConnectionStatus)
        {
            case ConnectionStatus.Connected:
            case ConnectionStatus.Connecting:
                {
                    string? countryCode = _connectionManager.CurrentConnectionDetails?.ExitCountryCode;
                    if (!string.IsNullOrEmpty(countryCode))
                    {
                        CurrentCountry = Countries.FirstOrDefault(c => c.Code == countryCode);
                    }
                    break;
                }
            case ConnectionStatus.Disconnected:
                CurrentCountry = Countries.FirstOrDefault(c => c.Code == _settings.DeviceLocation?.CountryCode);
                break;
        }
    }

    private void InvalidateCountries()
    {
        List<Country> countries = _serversCache.Countries
            .Select(c =>
            {
                (double Latitude, double Longitude)? coordinates = _coordinatesProvider.GetCoordinates(c);
                return coordinates != null
                    ? new Country
                    {
                        Name = Localizer.GetCountryName(c.Code),
                        Code = c.Code,
                        Latitude = coordinates.Value.Latitude,
                        Longitude = coordinates.Value.Longitude,
                        IsUnderMaintenance = c.IsStandardUnderMaintenance
                    }
                    : null;
            })
            .OfType<Country>()
            .ToList();

        // New-backend servers (GET /servers + GET /servers/pro, merged - see IFreeServersCache) carry
        // their own city-level coordinates, independent of the legacy per-country pins above - shown at
        // their exact location rather than the country's centroid, and skipped (not zeroed to 0,0) when
        // the backend hasn't populated location yet. Pro servers still get a pin (FreeServerIsForPaidUsersOnly
        // = true) - ConnectAsync below gates those behind the paid-plan upsell, same as a genuinely free
        // one (Free == 1) never does.
        countries.AddRange(_freeServersCache.GetServers()
            .Where(s => s.Location != null)
            .Select(s =>
            {
                string protocol = SyncVpnAccountClaimMapper.ResolveServerProtocol(_settings.VpnProtocol, s.Protocols);

                return new Country
                {
                    Name = s.Name,
                    Code = s.Country.ShortName,
                    Latitude = s.Location!.Lat,
                    Longitude = s.Location.Long,
                    IsUnderMaintenance = false,
                    IsFreeServer = true,
                    FreeServerId = s.Id,
                    FreeServerName = s.Name,
                    FreeServerProtocol = protocol,
                    FreeServerIsForPaidUsersOnly = s.Free == 0
                };
            }));

        Countries = countries;
    }

    [RelayCommand]
    private Task ConnectAsync(Country country)
    {
        if (country == null)
        {
            return Task.CompletedTask;
        }

        // Clear the selection now that a connect attempt is underway - covers both the map's own
        // Connect button (which passed this same country) and any other future caller, so a stale
        // "Connect to X" doesn't linger once X is being connected to (or an upsell/maintenance dialog
        // takes over below).
        if (SelectedCountry == country)
        {
            SelectedCountry = null;
        }

        // New-backend server pins connect straight to this exact server (SyncVpnServerLocationIntent),
        // matching the server rows elsewhere (Countries sidebar, Home free-servers section) - a genuinely
        // free one skips the paid-plan upsell gate entirely, but a Pro one still needs it.
        if (country.IsFreeServer)
        {
            if (country.FreeServerIsForPaidUsersOnly && !_settings.VpnPlan.IsPaid)
            {
                return _upsellCarouselWindowActivator.ActivateAsync(UpsellFeatureType.WorldwideCoverage);
            }

            return _connectionManager.ConnectAsync(
                VpnTriggerDimension.Map,
                new ConnectionIntent(new SyncVpnServerLocationIntent(
                    country.FreeServerId, country.FreeServerName, country.FreeServerProtocol,
                    isForPaidUsersOnly: country.FreeServerIsForPaidUsersOnly)));
        }

        if (!_settings.VpnPlan.IsPaid)
        {
            return _upsellCarouselWindowActivator.ActivateAsync(UpsellFeatureType.WorldwideCoverage);
        }

        if (country.IsUnderMaintenance)
        {
            return _mainWindowOverlayActivator.ShowMessageAsync(new()
            {
                Title =  Localizer.Get("Connections_Country_UnderMaintenance_Title"),
                Message =  Localizer.Get("Connections_Country_UnderMaintenance"),
                PrimaryButtonText = Localizer.Get("Common_Actions_GotIt"),
            });
        }

        return _connectionManager.ConnectAsync(VpnTriggerDimension.Map, new ConnectionIntent(SingleCountryLocationIntent.From(country.Code)));
    }
}