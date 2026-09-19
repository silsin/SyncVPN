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

using System.Data;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.GuestHole;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.FreeServers;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.SyncVpnServers;
using SyncVPN.Client.Logic.Connection.Contracts.RequestCreators;
using SyncVPN.Client.Logic.Connection.Extensions;
using SyncVPN.Client.Logic.Connection.GuestHole;
using SyncVPN.Client.Logic.Connection.Statistics;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Enums;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Logic.Services.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Common.Core.Networking;
using SyncVPN.Crypto.Contracts;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;
using SyncVPN.Logging.Contracts.Events.ConnectionLogs;
using SyncVPN.Logging.Contracts.Events.ConnectLogs;
using SyncVPN.ProcessCommunication.Contracts.Entities.Crypto;
using SyncVPN.ProcessCommunication.Contracts.Entities.LocalAgent;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;
using ConnectionDetails = SyncVPN.Client.Logic.Connection.Contracts.Models.ConnectionDetails;
using IpAddressInfo = SyncVPN.Common.Core.Vpn.IpAddressInfo;

namespace SyncVPN.Client.Logic.Connection;

public class ConnectionManager : IInternalConnectionManager, IGuestHoleConnector,
    IEventMessageReceiver<ConnectionDetailsIpcEntity>,
    IEventMessageReceiver<ConnectionCertificateUpdatedMessage>,
    IEventMessageReceiver<GuestHoleStatusChangedMessage>
{
    private readonly TimeSpan _reconnectInterval = TimeSpan.FromMinutes(1);
    private readonly Random _random = new();

    // Safety net for when the native service's own connect/ping-and-retry logic never reaches a
    // terminal state at all (e.g. a native reachability probe that doesn't honor its own timeout - see
    // UdpPingClient.PingAsync) - without this, a stuck probe leaves the UI on "Connecting" forever with
    // no error and no way out except a manual disconnect. 45s is generous enough to not preempt a
    // legitimately slow multi-server fallback, since VpnEndpointScanner already bounds each individual
    // probe to a few seconds.
    private static readonly TimeSpan ConnectingWatchdogTimeout = TimeSpan.FromSeconds(45);
    private CancellationTokenSource? _connectingWatchdogCts;

    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IEntityMapper _entityMapper;
    private readonly IConnectionRequestCreator _connectionRequestCreator;
    private readonly IReconnectionRequestCreator _reconnectionRequestCreator;
    private readonly IDisconnectionRequestCreator _disconnectionRequestCreator;
    private readonly IServersLoader _serversLoader;
    private readonly IFavoriteServersStorage _favoriteServersStorage;
    private readonly IGuestHoleServersFileStorage _guestHoleServersFileStorage;
    private readonly IGuestHoleConnectionRequestCreator _guestHoleConnectionRequestCreator;
    private readonly IConnectionStatisticalEventsManager _statisticalEventManager;
    private readonly IConnectionKeyManager _connectionKeyManager;

    private DateTime _minReconnectionDateUtc = DateTime.MinValue;

    private bool _isNetworkBlocked;
    private bool _isConnectionStatusHandled;
    private bool _isGuestHoleActive;

    private VpnStateIpcEntity? _cachedMessage;
    private IpAddressInfo? _cachedServerIpAddress;

    private VpnStatusIpcEntity? _currentStatus = VpnStatusIpcEntity.Disconnected;
    private VpnErrorTypeIpcEntity? _currentError = VpnErrorTypeIpcEntity.None;

    public ConnectionStatus ConnectionStatus { get; private set; }
    public IConnectionIntent? CurrentConnectionIntent { get; private set; }
    public ConnectionDetails? CurrentConnectionDetails { get; private set; }
    public VpnTriggerDimension? CurrentConnectionTrigger { get; private set; }

    public bool IsDisconnected => ConnectionStatus == ConnectionStatus.Disconnected;
    public bool IsConnecting => ConnectionStatus == ConnectionStatus.Connecting;
    public bool IsConnected => ConnectionStatus == ConnectionStatus.Connected;
    public bool HasError => _currentError.HasError();
    public bool IsNetworkBlocked => _isNetworkBlocked;
    public bool IsTwoFactorError => !IsDisconnected && _currentError.IsTwoFactorError();
    public bool IsMobileHotspotError => _currentError == VpnErrorTypeIpcEntity.InterfaceHasForwardingEnabled;

    public ConnectionManager(
        ILogger logger,
        ISettings settings,
        IVpnServiceCaller vpnServiceCaller,
        IEventMessageSender eventMessageSender,
        IEntityMapper entityMapper,
        IConnectionRequestCreator connectionRequestCreator,
        IReconnectionRequestCreator reconnectionRequestCreator,
        IDisconnectionRequestCreator disconnectionRequestCreator,
        IServersLoader serversLoader,
        IFavoriteServersStorage favoriteServersStorage,
        IGuestHoleServersFileStorage guestHoleServersFileStorage,
        IGuestHoleConnectionRequestCreator guestHoleConnectionRequestCreator,
        IConnectionStatisticalEventsManager statisticalEventManager,
        IConnectionKeyManager connectionKeyManager)
    {
        _logger = logger;
        _settings = settings;
        _vpnServiceCaller = vpnServiceCaller;
        _eventMessageSender = eventMessageSender;
        _entityMapper = entityMapper;
        _connectionRequestCreator = connectionRequestCreator;
        _reconnectionRequestCreator = reconnectionRequestCreator;
        _disconnectionRequestCreator = disconnectionRequestCreator;
        _serversLoader = serversLoader;
        _favoriteServersStorage = favoriteServersStorage;
        _guestHoleServersFileStorage = guestHoleServersFileStorage;
        _guestHoleConnectionRequestCreator = guestHoleConnectionRequestCreator;
        _guestHoleConnectionRequestCreator = guestHoleConnectionRequestCreator;
        _statisticalEventManager = statisticalEventManager;
        _connectionKeyManager = connectionKeyManager;
    }

    public async Task ConnectAsync(
        VpnTriggerDimension connectionTrigger,
        IConnectionIntent? connectionIntent = null)
    {
        _statisticalEventManager.SetConnectionAttempt(connectionTrigger, ConnectionStatus);

        connectionIntent ??= _settings.VpnPlan.IsPaid ? ConnectionIntent.Default : ConnectionIntent.FreeDefault;
        connectionIntent = ChangeConnectionIntent(connectionIntent, CreateNewIntentIfUserPlanIsFree);

        CurrentConnectionIntent = connectionIntent;
        CurrentConnectionTrigger = connectionTrigger;

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Connection attempt to: {connectionIntent}. Triggered by {connectionTrigger}.", stackTraceDepth: 2);

        ConnectionRequestIpcEntity? request = await TryCreateRequestAsync(() => _connectionRequestCreator.CreateAsync(connectionIntent));
        if (request is null)
        {
            return;
        }

        await SendRequestIfValidAsync(request);
    }

    // Building the request can fail outside the usual VpnError validation path below - e.g. the new
    // SyncVPN backend's POST /account rejecting the claim (rate limited, maintenance, etc.) throws
    // rather than producing a request to validate. Catching here keeps that failure from reaching the
    // UI as an unhandled exception (which crashes the whole app) - it's surfaced the same way as any
    // other connection failure instead, via the existing ConnectionErrorMessage/error banner.
    private async Task<ConnectionRequestIpcEntity?> TryCreateRequestAsync(Func<Task<ConnectionRequestIpcEntity>> createRequest)
    {
        try
        {
            return await createRequest();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.Error<ConnectionErrorLog>("Failed to create the connection request.", ex);
            _eventMessageSender.Send(new ConnectionErrorMessage { VpnError = VpnError.Unknown });
            return null;
        }
    }

    public async Task ConnectToGuestHoleAsync()
    {
        IOrderedEnumerable<GuestHoleServerContract> servers = (await _guestHoleServersFileStorage.GetAsync()).OrderBy(_ => _random.Next());
        if (!servers.Any())
        {
            throw new GuestHoleException("No guest hole servers provided.");
        }

        ConnectionRequestIpcEntity request = await _guestHoleConnectionRequestCreator.CreateAsync(servers);

        _logger.Info<ConnectTriggerLog>("Guest hole connection requested.");
        await _vpnServiceCaller.ConnectAsync(request);
    }

    public async Task DisconnectFromGuestHoleAsync()
    {
        DisconnectionRequestIpcEntity request = _disconnectionRequestCreator.Create(VpnError.NoneKeepEnabledKillSwitch);

        await _vpnServiceCaller.DisconnectAsync(request);
    }

    private IConnectionIntent CreateNewIntentIfUserPlanIsFree(IConnectionIntent connectionIntent)
    {
        if (_settings.VpnPlan.IsPaid)
        {
            return connectionIntent;
        }

        ILocationIntent locationIntent = connectionIntent.Location.IsForPaidUsersOnly
            ? FreeServerLocationIntent.Default
            : connectionIntent.Location;

        IFeatureIntent? featureIntent = connectionIntent.Feature is null || connectionIntent.Feature.IsForPaidUsersOnly
            ? null
            : connectionIntent.Feature;

        return new ConnectionIntent(locationIntent, featureIntent);
    }

    private async Task<bool> SendRequestIfValidAsync(ConnectionRequestIpcEntity request)
    {
        VpnError error = request.GetVpnError();
        if (error == VpnError.None)
        {
            await _vpnServiceCaller.ConnectAsync(request);
            return true;
        }
        else
        {
            _logger.Error<ConnectionErrorLog>($"Failed to connect due to '{error}' error detected.");

            await DisconnectAsync(VpnTriggerDimension.Auto);

            _eventMessageSender.Send(new ConnectionErrorMessage { VpnError = error });
            return false;
        }
    }

    /// <returns>True if reconnecting. False if not.</returns>
    public async Task<bool> ReconnectIfNotRecentlyReconnectedAsync()
    {
        if (DateTime.UtcNow > _minReconnectionDateUtc)
        {
            return await ReconnectAsync(VpnTriggerDimension.Auto);
        }

        return false;
    }

    /// <summary>Reconnects if the most recent action was a Connect and not a Disconnect.</summary>
    /// <returns>True if reconnecting. False if not.</returns>
    public async Task<bool> ReconnectAsync(VpnTriggerDimension reconnectionTrigger)
    {
        // If there is no internet connection or the attempt to reach the guest hole servers fails,
        // we should not trigger reconnection logic, since all guest hole servers have already been tried.
        if (_isGuestHoleActive)
        {
            return false;
        }

        _minReconnectionDateUtc = DateTime.UtcNow + _reconnectInterval;

        IConnectionIntent? connectionIntent = CurrentConnectionIntent;
        if (connectionIntent is null)
        {
            await DisconnectAsync(VpnTriggerDimension.Auto);
            return false;
        }

        _statisticalEventManager.SetReconnectionAttempt(reconnectionTrigger, ConnectionStatus);

        connectionIntent = ChangeConnectionIntent(connectionIntent, CreateNewIntentIfUserPlanIsFree);

        CurrentConnectionIntent = connectionIntent;

        // An Auto reconnect (retrying the next candidate endpoint, recovering from a transient error) is
        // a transparent continuation of whichever button originally started this attempt, not a new
        // action - it must not steal that button's "I started this" disabled state (see
        // ConnectionCardComponentViewModel.IsConnectingViaThisButton) by overwriting it here. Only a
        // reconnect with a real, non-Auto trigger (e.g. NewConnection from a settings/profile change)
        // represents a genuinely fresh attempt worth re-recording.
        if (reconnectionTrigger != VpnTriggerDimension.Auto)
        {
            CurrentConnectionTrigger = reconnectionTrigger;
        }

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Reconnection attempt to: {connectionIntent}. Triggered by {reconnectionTrigger}.", stackTraceDepth: 1);

        ConnectionRequestIpcEntity? request = await TryCreateRequestAsync(() => _reconnectionRequestCreator.CreateAsync(connectionIntent));
        if (request is null)
        {
            return false;
        }

        return await SendRequestIfValidAsync(request);
    }

    public async Task DisconnectAsync(VpnTriggerDimension disconnectionTrigger)
    {
        _statisticalEventManager.SetDisconnectionAttempt(disconnectionTrigger, ConnectionStatus);

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Disconnection attempt. Triggered by {disconnectionTrigger}.", stackTraceDepth: 2);

        CurrentConnectionIntent = null;
        CurrentConnectionTrigger = null;

        DisconnectionRequestIpcEntity request = _disconnectionRequestCreator.Create();

        await _vpnServiceCaller.DisconnectAsync(request);
    }

    public async Task HandleAsync(VpnStateIpcEntity message)
    {
        _cachedMessage = message;

        IConnectionIntent connectionIntent = CurrentConnectionIntent ?? ConnectionIntent.Default;
        bool isToForceStatusUpdate = _isNetworkBlocked != message.NetworkBlocked || !_isConnectionStatusHandled;

        _isConnectionStatusHandled = true;
        _isNetworkBlocked = message.NetworkBlocked;

        if (!_isGuestHoleActive)
        {
            if (message.Status is VpnStatusIpcEntity.Pinging or VpnStatusIpcEntity.Connected)
            {
                VpnProtocol vpnProtocol = _entityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(message.VpnProtocol);
                Server? server = GetCurrentServer(message, vpnProtocol);
                PhysicalServer? physicalServer = server?.Servers.FirstOrDefault(FilterPhysicalServerByVpnState(message, vpnProtocol));

                // A SyncVPN-catalog server only ever exists in the new backend's catalog - it can never
                // be found by GetCurrentServer above, which only searches _serversLoader's legacy Proton
                // cache. Without this, every single SyncVPN server connection actually succeeded at the
                // native/tunnel level (a real EndpointIp was reported) but fell into the "Server is null"
                // branch below on every status update, endlessly reconnecting until the native service
                // gave out - surfacing to the user as a generic Unknown (VpnError 14) failure for a
                // connection that had genuinely worked.
                //
                // Two intents can lead here: SyncVpnServerLocationIntent (one specific server picked by
                // id, e.g. from the map) carries its own ServerId/ServerName; FreeServerLocationIntent
                // ("Fastest"/"Random" free server, e.g. the Connection Card's default button) is a
                // strategy rather than a specific server, so it has neither - the native status report's
                // own Label/EndpointIp is the only identity available for that case (VPNWIN-2105).
                if (server is null)
                {
                    if (connectionIntent.Location is SyncVpnServerLocationIntent syncVpnIntent)
                    {
                        (server, physicalServer) = BuildSyncVpnServer(syncVpnIntent.ServerId.ToString(), syncVpnIntent.ServerName, syncVpnIntent.IsForPaidUsersOnly, message);
                    }
                    else if (connectionIntent.Location is FreeServerLocationIntent && !string.IsNullOrEmpty(message.EndpointIp))
                    {
                        string displayName = string.IsNullOrEmpty(message.Label) ? message.EndpointIp : message.Label;
                        (server, physicalServer) = BuildSyncVpnServer(message.EndpointIp, displayName, isForPaidUsersOnly: false, message);
                    }
                }

                if (server is not null && physicalServer is not null)
                {
                    _favoriteServersStorage.SetCurrentServerId(server.Id);

                    if (CurrentConnectionDetails is null || !CurrentConnectionDetails.OriginalConnectionIntent.IsSameAs(connectionIntent))
                    {
                        CurrentConnectionDetails = new ConnectionDetails(
                            connectionIntent,
                            server,
                            physicalServer,
                            vpnProtocol,
                            message.EndpointPort);
                    }
                    else
                    {
                        CurrentConnectionDetails.UpdateServer(server, physicalServer, vpnProtocol, message.EndpointPort);
                    }

                    if (_cachedServerIpAddress is not null)
                    {
                        CurrentConnectionDetails.UpdateServerIpAddress(_cachedServerIpAddress.Value);
                        _cachedServerIpAddress = null;
                    }
                }
                else if (server is null)
                {
                    _logger.Error<AppLog>($"The status changed to Connected but the associated Server is null. Error: '{message.Error}' " +
                                            $"NetworkBlocked: '{message.NetworkBlocked}' " +
                                            $"Status: '{message.Status}' EntryIp: '{message.EndpointIp}' Label: '{message.Label}' " +
                                            $"NetworkAdapterType: '{message.OpenVpnAdapterType}' VpnProtocol: '{message.VpnProtocol}'");

                    // VPNWIN-2105 - Either (1) Reconnect without last server, or (2) Delete this comment
                    await ReconnectAsync(VpnTriggerDimension.Auto);
                }
                else // Tier is too low for the connected server
                {
                    await ReconnectIfNotRecentlyReconnectedAsync();
                }
            }
            else if (message.Status == VpnStatusIpcEntity.Disconnected)
            {
                CurrentConnectionDetails = null;
                _favoriteServersStorage.SetCurrentServerId(null);
            }
        }

        if (message.Status != VpnStatusIpcEntity.ActionRequired ||
            message.Error.IsTwoFactorError())
        {
            SetConnectionStatus(message.Status, message.Error, isToForceStatusUpdate);
        }
    }

    private Server? GetCurrentServer(VpnStateIpcEntity state, VpnProtocol vpnProtocol)
    {
        return _serversLoader.GetServers().FirstOrDefault(s => s.Servers.Any(FilterPhysicalServerByVpnState(state, vpnProtocol)));
    }

    // Built entirely from data already available here (an identity - either the intent's own, or the
    // native status report's Label/EndpointIp when the intent doesn't carry one - plus the native
    // service's own status report) rather than looked up anywhere, since this server doesn't exist in
    // any legacy cache to look up from. Geographic fields (City/State/ExitCountry/etc.) are
    // intentionally left blank - this layer has no access to the new backend's server catalog
    // (IFreeServersCache lives in a higher layer than this project) to fill them in correctly; only
    // what's needed for a valid, non-crashing ConnectionDetails is populated.
    private static (Server Server, PhysicalServer PhysicalServer) BuildSyncVpnServer(string id, string name, bool isForPaidUsersOnly, VpnStateIpcEntity state)
    {
        PhysicalServer physicalServer = new()
        {
            Id = id,
            EntryIp = state.EndpointIp,
            ExitIp = state.EndpointIp,
            Domain = state.Label,
            Label = state.Label,
            Status = 1,
            X25519PublicKey = string.Empty,
            Signature = string.Empty,
        };

        Server server = new()
        {
            Id = id,
            Name = name,
            City = string.Empty,
            State = string.Empty,
            EntryCountry = string.Empty,
            ExitCountry = string.Empty,
            HostCountry = string.Empty,
            Domain = state.Label,
            Status = 1,
            Tier = isForPaidUsersOnly ? ServerTiers.Basic : ServerTiers.Free,
            Features = default,
            Load = 0,
            Score = 0,
            Servers = [physicalServer],
            IsVirtual = false,
            GatewayName = string.Empty,
            StatusReference = new StatusReference(),
            EntryLocation = new GeoLocation(),
            ExitLocation = new GeoLocation(),
        };

        return (server, physicalServer);
    }

    private Func<PhysicalServer, bool> FilterPhysicalServerByVpnState(VpnStateIpcEntity state, VpnProtocol vpnProtocol)
    {
        return physicalServer => physicalServer.Label == state.Label
            && (physicalServer.EntryIp == state.EndpointIp ||
                (physicalServer.RelayIpByProtocol is not null &&
                 physicalServer.RelayIpByProtocol.ContainsKey(vpnProtocol) &&
                 physicalServer.RelayIpByProtocol[vpnProtocol] == state.EndpointIp));
    }

    private void SetConnectionStatus(
        VpnStatusIpcEntity status,
        VpnErrorTypeIpcEntity error,
        bool forceSendStatusUpdate = false)
    {
        if (_currentStatus == status && _currentError == error && !forceSendStatusUpdate)
        {
            return;
        }

        _currentStatus = status;
        _currentError = error;

        CurrentConnectionDetails?.UpdateStatus(status);

        ConnectionStatus = MapConnectionStatus(status, error);

        // ActionRequired (waiting on a 2FA code) also maps to ConnectionStatus.Connecting, but that's
        // bounded by the user typing, not the service - it must never be auto-timed-out. Arm state is
        // tracked by whether the CTS exists rather than by the previous ConnectionStatus, since both
        // Pinging and ActionRequired map to the same Connecting value and would otherwise make a
        // 2FA-then-back-to-connecting transition fail to re-arm.
        bool shouldWatchdogBeArmed = ConnectionStatus == ConnectionStatus.Connecting && status != VpnStatusIpcEntity.ActionRequired;

        if (shouldWatchdogBeArmed && _connectingWatchdogCts is null)
        {
            ArmConnectingWatchdog();
        }
        else if (!shouldWatchdogBeArmed && _connectingWatchdogCts is not null)
        {
            DisarmConnectingWatchdog();
        }

        _eventMessageSender.Send(new ConnectionStatusChangedMessage(ConnectionStatus));

        _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] Status updated to {ConnectionStatus}{(_isGuestHoleActive ? " (Guest hole)" : string.Empty)}.{(IsConnected ? $" Connected to server {CurrentConnectionDetails?.ServerName}" : string.Empty)}");

        if (error != VpnErrorTypeIpcEntity.None)
        {
            _logger.Error<ConnectTriggerLog>($"[CONNECTION_PROCESS] Connection error reason: '{error}' (native status '{status}', server '{CurrentConnectionDetails?.ServerName}').");
        }

        _statisticalEventManager.OnVpnStateChanged(status, error, CurrentConnectionDetails);
    }

    private ConnectionStatus MapConnectionStatus(VpnStatusIpcEntity status, VpnErrorTypeIpcEntity error)
    {
        return status == VpnStatusIpcEntity.ActionRequired && error.IsTwoFactorError()
            ? ConnectionStatus.Connecting
            : _entityMapper.Map<VpnStatusIpcEntity, ConnectionStatus>(status);
    }

    private void ArmConnectingWatchdog()
    {
        _connectingWatchdogCts = new CancellationTokenSource();

        RunConnectingWatchdogAsync(_connectingWatchdogCts.Token).FireAndForget();
    }

    private void DisarmConnectingWatchdog()
    {
        _connectingWatchdogCts?.Cancel();
        _connectingWatchdogCts?.Dispose();
        _connectingWatchdogCts = null;
    }

    private async Task RunConnectingWatchdogAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(ConnectingWatchdogTimeout, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await OnConnectingWatchdogElapsedAsync();
    }

    private async Task OnConnectingWatchdogElapsedAsync()
    {
        _logger.Warn<ConnectTriggerLog>(
            $"[CONNECTION_PROCESS] Still Connecting {ConnectingWatchdogTimeout.TotalSeconds:0}s after the attempt started with no terminal status from the service - giving up and disconnecting.");

        DisarmConnectingWatchdog();

        _statisticalEventManager.SetDisconnectionAttempt(VpnTriggerDimension.Auto, ConnectionStatus);

        CurrentConnectionIntent = null;
        CurrentConnectionTrigger = null;

        DisconnectionRequestIpcEntity request = _disconnectionRequestCreator.Create(VpnError.PingTimeoutError);
        await _vpnServiceCaller.DisconnectAsync(request);
    }

    public void Receive(ConnectionDetailsIpcEntity message)
    {
        IpAddressInfo serverIpAddress = _entityMapper.Map<VpnServerAddressIpcEntity, IpAddressInfo>(message.ServerIpAddress);
        _cachedServerIpAddress = serverIpAddress;

        CurrentConnectionDetails?.UpdateServerIpAddress(serverIpAddress);

        _eventMessageSender.Send(new ConnectionDetailsChangedMessage
        {
            ClientCountryCode = message.ClientCountryIsoCode,
            ClientIpAddress = message.ClientIpAddress,
            ServerIpAddress = serverIpAddress,
        });
    }

    public async void Receive(ConnectionCertificateUpdatedMessage message)
    {
        AsymmetricKeyPair? clientKeyPair = _connectionKeyManager.GetKeyPairOrNull();

        if (message.Certificate is not null && clientKeyPair is not null)
        {
            await _vpnServiceCaller.UpdateLocalAgentTlsCredentialsAsync(new LocalAgentTlsCredentialsIpcEntity()
            {
                ConnectionCertificate = new ConnectionCertificateIpcEntity()
                {
                    Pem = message.Certificate.Value.Pem,
                    ExpirationDateUtc = message.Certificate.Value.ExpirationUtcDate.UtcDateTime,
                },
                ClientKeyPair = _entityMapper.Map<AsymmetricKeyPair, AsymmetricKeyPairIpcEntity>(clientKeyPair),
            });
        }
    }

    public async Task InitializeAsync(IConnectionIntent? connectionIntent)
    {
        CurrentConnectionIntent = connectionIntent;

        if (_cachedMessage is not null)
        {
            await HandleAsync(_cachedMessage);
        }

        await _vpnServiceCaller.RequestConnectionDetailsAsync();
    }

    public void Receive(GuestHoleStatusChangedMessage message)
    {
        _isGuestHoleActive = message.IsActive;
    }

    private IConnectionIntent ChangeConnectionIntent(IConnectionIntent connectionIntent, Func<IConnectionIntent, IConnectionIntent> changeIntentFunc)
    {
        IConnectionIntent newConnectionIntent = changeIntentFunc(connectionIntent);
        if (newConnectionIntent != connectionIntent)
        {
            _logger.Info<ConnectTriggerLog>($"[CONNECTION_PROCESS] The connection intent is changing from " +
                                            $"{connectionIntent} to {newConnectionIntent}.");
        }

        return newConnectionIntent;
    }
}