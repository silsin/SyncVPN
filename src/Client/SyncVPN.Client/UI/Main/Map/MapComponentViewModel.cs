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
using SyncVPN.Client.Logic.Services.Contracts;
using SyncVPN.Client.Logic.Services.Contracts.Messages;
using SyncVPN.Client.Services.FreeServers;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.Logging.Contracts.Events.AppLogs;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.UI.Main.Map;

public partial class MapComponentViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<MapLocationSelectedMessage>,
    IEventMessageReceiver<ServiceEnablementChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IServersCache _serversCache;
    private readonly IFreeServersCache _freeServersCache;
    private readonly IConnectionManager _connectionManager;
    private readonly ICoordinatesProvider _coordinatesProvider;
    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IServiceManager _serviceManager;

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
    [NotifyPropertyChangedFor(nameof(ShowSelectedConnectButton))]
    private Country? _selectedCountry;

    partial void OnSelectedCountryChanged(Country? value)
    {
        if (value is null)
        {
            return;
        }

        Logger.Info<AppLog>("Map: server item selected - " + DescribeCountry(value));
    }

    public bool HasSelectedCountry => SelectedCountry is not null;

    public string ConnectButtonText => SelectedCountry is not null
        ? string.Format(Localizer.Get("Map_ConnectTo"), SelectedCountry.Name)
        : string.Empty;

    public bool IsConnecting => _connectionManager.IsConnecting;
    public bool IsConnected => _connectionManager.IsConnected;
    public bool IsDisconnected => _connectionManager.IsDisconnected;

    // True only while the in-progress connection was started via one of THIS map's own buttons - either
    // MapSelectedConnectButton (a specific pin picked) or MapDefaultConnectButton (no pin picked, falls
    // back to fastest - see ConnectAsync(Country) below) - lets that button stay visible but disabled
    // instead of disappearing, while the OTHER surface (the bottom bar's Fastest server card) is the one
    // that switches to Cancel. See ConnectionCardComponentViewModel.IsConnectingViaThisButton for the mirror.
    public bool IsConnectingFromMapSelection => IsConnecting && _connectionManager.CurrentConnectionTrigger == VpnTriggerDimension.Map;

    // Hidden (not just disabled) while the service is unavailable - same as the bottom bar's own Connect
    // button (see ConnectionCardComponentViewModel.ShowConnectButton/ShowEnableServiceButton), which is
    // the one place that offers the "Enable" action, so the map doesn't need a second copy of it.
    public bool ShowConnectButton => (IsDisconnected && _serviceManager.IsServiceEnabled) || IsConnectingFromMapSelection;

    public bool ShowSelectedConnectButton => HasSelectedCountry && (_serviceManager.IsServiceEnabled || IsConnectingFromMapSelection);

    public bool ShowCancelButton => IsConnecting && !IsConnectingFromMapSelection;

    // The bottom bar's own Connect button (ConnectionCardComponentViewModel.CanConnect) already refuses
    // to connect while the Windows service is unavailable - this screen's Connect buttons (map pin /
    // "fastest") bypassed that check entirely and went straight to ConnectionManager.ConnectAsync, which
    // has no service check of its own, so pressing them with the service down just failed deep in the
    // connection pipeline with a confusing error instead of being blocked up front.
    public bool CanConnectFromMap => !IsConnectingFromMapSelection && _serviceManager.IsServiceEnabled;

    public MapComponentViewModel(
        ISettings settings,
        IServersCache serversCache,
        IFreeServersCache freeServersCache,
        IConnectionManager connectionManager,
        ICoordinatesProvider coordinatesProvider,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IServiceManager serviceManager,
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
        _serviceManager = serviceManager;

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
            // would be stale. The one exception is while OUR OWN selection's Connect button is what
            // started this attempt (trigger == Map) - there it must stay put (see
            // IsConnectingFromMapSelection) instead of vanishing mid-connect.
            bool isOwnSelectionConnecting = message.ConnectionStatus == ConnectionStatus.Connecting
                && _connectionManager.CurrentConnectionTrigger == VpnTriggerDimension.Map;

            if (message.ConnectionStatus != ConnectionStatus.Disconnected && !isOwnSelectionConnecting)
            {
                SelectedCountry = null;
            }

            OnPropertyChanged(nameof(IsConnecting));
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(IsDisconnected));
            OnPropertyChanged(nameof(IsConnectingFromMapSelection));
            OnPropertyChanged(nameof(ShowConnectButton));
            OnPropertyChanged(nameof(ShowSelectedConnectButton));
            OnPropertyChanged(nameof(ShowCancelButton));
            OnPropertyChanged(nameof(CanConnectFromMap));
        });
    }

    public void Receive(ServiceEnablementChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(ShowConnectButton));
            OnPropertyChanged(nameof(ShowSelectedConnectButton));
            OnPropertyChanged(nameof(CanConnectFromMap));
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
                    Country? activeCountry = ResolveActiveCountry();
                    if (activeCountry != null)
                    {
                        CurrentCountry = activeCountry;
                    }
                    break;
                }
            case ConnectionStatus.Disconnected:
                CurrentCountry = Countries.FirstOrDefault(c => c.Code == _settings.DeviceLocation?.CountryCode);
                break;
        }
    }

    // The new SyncVPN backend's per-server intent (SyncVpnServerLocationIntent) carries only a
    // ServerId, no country - and ConnectionManager.BuildSyncVpnServer leaves the synthesized Server's
    // ExitCountry blank, since that layer has no access to the free-servers catalog to fill it in (see
    // its own comment). So CurrentConnectionDetails.ExitCountryCode is always empty for these servers,
    // which used to mean the map never highlighted the pin actually being connected to. Falling back to
    // matching CurrentConnectionIntent's ServerId against our own Countries list (built from
    // IFreeServersCache, which this layer does have access to) is what lets the correct pin animate
    // through connecting/connected for them.
    private Country? ResolveActiveCountry()
    {
        string? countryCode = _connectionManager.CurrentConnectionDetails?.ExitCountryCode;
        if (!string.IsNullOrEmpty(countryCode))
        {
            return Countries.FirstOrDefault(c => c.Code == countryCode);
        }

        return _connectionManager.CurrentConnectionIntent?.Location is SyncVpnServerLocationIntent syncVpnIntent
            ? Countries.FirstOrDefault(c => c.IsFreeServer && c.FreeServerId == syncVpnIntent.ServerId)
            : null;
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
                (string protocol, string? transport) = SyncVpnAccountClaimMapper.ResolveServerProtocolAndTransport(_settings.VpnProtocol, s.Protocols);

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
                    FreeServerTransport = transport,
                    FreeServerIsForPaidUsersOnly = s.Free == 0
                };
            }));

        Countries = countries;
    }

    [RelayCommand]
    private Task ConnectAsync(Country country)
    {
        // Defense in depth: CanConnectFromMap already disables both map Connect buttons for this, but a
        // click that raced the button's IsEnabled re-evaluation (e.g. right as the service goes down)
        // should not be allowed to reach ConnectionManager.ConnectAsync, which has no service check of
        // its own and would otherwise fail deep in the connection pipeline with a confusing error.
        if (!_serviceManager.IsServiceEnabled)
        {
            Logger.Info<AppLog>("Map: Connect pressed while the Windows service is unavailable - ignoring.");
            return Task.CompletedTask;
        }

        if (country == null)
        {
            // No pin picked (tap or Free-servers row) - fall back to the app's usual "fastest" pick,
            // same as the main Connect button uses via ConnectionManager.ConnectAsync's own null
            // fallback (ConnectionIntent.Default / FreeDefault, per the user's plan). The map itself
            // then picks up and highlights whichever server actually got picked once ConnectionRequestCreator
            // resolves it - see ClaimFreeServerAsync's MapLocationSelectedMessage and
            // MapComponentViewModel.ResolveActiveCountry.
            Logger.Info<AppLog>("Map: Connect pressed with no server selected - falling back to fastest.");
            return _connectionManager.ConnectAsync(VpnTriggerDimension.Map);
        }

        Logger.Info<AppLog>("Map: Connect pressed - " + DescribeCountry(country));

        // Deliberately NOT clearing SelectedCountry here - Receive(ConnectionStatusChangedMessage)
        // already clears it once the status actually leaves Disconnected, which is the same message
        // ConnectionCardComponentViewModel reacts to for its own Connect/Cancel/Disconnect button group.
        // Clearing it synchronously here instead used to flip HasSelectedCountry to false immediately on
        // press, swapping the card over to that OTHER view model's state before its IsConnecting had
        // caught up - flashing "Please select a server" for a frame until the real status change arrived
        // a moment later. Waiting for the shared message keeps both in lockstep.

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
                    transport: country.FreeServerTransport,
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

    private static string DescribeCountry(Country country)
    {
        return $"Name='{country.Name}', Code='{country.Code}', IsFreeServer={country.IsFreeServer}, " +
               $"FreeServerId={country.FreeServerId}, FreeServerName='{country.FreeServerName}', " +
               $"FreeServerProtocol='{country.FreeServerProtocol}', ForPaidUsersOnly={country.FreeServerIsForPaidUsersOnly}, " +
               $"IsUnderMaintenance={country.IsUnderMaintenance}, Lat={country.Latitude}, Lng={country.Longitude}";
    }
}