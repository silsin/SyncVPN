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

using NSubstitute;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using SyncVPN.Client.Logic.Connection.Contracts.RequestCreators;
using SyncVPN.Client.Logic.Connection.GuestHole;
using SyncVPN.Client.Logic.Connection.Statistics;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Services.Contracts;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Crypto;
using SyncVPN.ProcessCommunication.Contracts.Entities.LocalAgent;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Logic.Connection.Tests;

[TestClass]
public class ConnectionManagerTest
{
    private const int PAID_PLAN_TIER = 2;

    private ILogger? _logger;
    private ISettings? _settings;
    private IVpnServiceCaller? _vpnServiceCaller;
    private IEventMessageSender? _eventMessageSender;
    private IEntityMapper? _entityMapper;
    private IConnectionRequestCreator? _connectionRequestCreator;
    private IReconnectionRequestCreator? _reconnectionRequestCreator;
    private IDisconnectionRequestCreator? _disconnectionRequestCreator;
    private IServersLoader? _serversLoader;
    private IFavoriteServersStorage? _favoriteServersStorage;
    private IGuestHoleServersFileStorage? _guestHoleServersFileStorage;
    private IGuestHoleConnectionRequestCreator? _guestHoleConnectionRequestCreator;
    private IConnectionStatisticalEventsManager? _statisticalEventManager;
    private IConnectionKeyManager? _connectionKeyManager;
    private IServiceManager? _serviceManager;

    [TestInitialize]
    public void Initialize()
    {
        _logger = Substitute.For<ILogger>();
        _settings = Substitute.For<ISettings>();
        _vpnServiceCaller = Substitute.For<IVpnServiceCaller>();
        _eventMessageSender = Substitute.For<IEventMessageSender>();
        _entityMapper = Substitute.For<IEntityMapper>();
        _connectionRequestCreator = Substitute.For<IConnectionRequestCreator>();
        _reconnectionRequestCreator = Substitute.For<IReconnectionRequestCreator>();
        _disconnectionRequestCreator = Substitute.For<IDisconnectionRequestCreator>();
        _serversLoader = Substitute.For<IServersLoader>();
        _favoriteServersStorage = Substitute.For<IFavoriteServersStorage>();
        _guestHoleServersFileStorage = Substitute.For<IGuestHoleServersFileStorage>();
        _guestHoleConnectionRequestCreator = Substitute.For<IGuestHoleConnectionRequestCreator>();
        _statisticalEventManager = Substitute.For<IConnectionStatisticalEventsManager>();
        _connectionKeyManager = Substitute.For<IConnectionKeyManager>();
        _serviceManager = Substitute.For<IServiceManager>();
        _serviceManager.IsServiceEnabled.Returns(true);

        _connectionRequestCreator!.CreateAsync(Arg.Any<IConnectionIntent>()).Returns(GetConnectionRequestIpcEntity());
        _reconnectionRequestCreator!.CreateAsync(Arg.Any<IConnectionIntent>()).Returns(GetConnectionRequestIpcEntity());
    }

    [TestCleanup]
    public virtual void Cleanup()
    {
        _logger = null;
        _settings = null;
        _vpnServiceCaller = null;
        _eventMessageSender = null;
        _entityMapper = null;
        _connectionRequestCreator = null;
        _reconnectionRequestCreator = null;
        _disconnectionRequestCreator = null;
        _serversLoader = null;
        _favoriteServersStorage = null;
        _guestHoleServersFileStorage = null;
        _guestHoleConnectionRequestCreator = null;
        _statisticalEventManager = null;
        _connectionKeyManager = null;
        _serviceManager = null;
    }

    [TestMethod]
    [DataRow(typeof(SecureCoreFeatureIntent))]
    [DataRow(typeof(TorFeatureIntent))]
    public async Task ConnectAsync_ShouldNot_ChangeConnectionIntentWhenPortForwardingEnabledAsync(Type featureIntentType)
    {
        // Arrange
        _settings!.IsPortForwardingEnabled.Returns(true);
        _settings!.VpnPlan.Returns(new VpnPlan(string.Empty, string.Empty, PAID_PLAN_TIER, false));

        ConnectionManager connectionManager = GetConnectionManager();
        IFeatureIntent featureIntent = GetFeatureIntent(featureIntentType);
        IConnectionIntent connectionIntent = GetConnectionIntent(featureIntent);

        // Act
        await connectionManager.ConnectAsync(VpnTriggerDimension.Auto, connectionIntent);

        // Assert
        Assert.IsTrue(connectionManager.CurrentConnectionIntent?.IsSameAs(connectionIntent));
    }

    [TestMethod]
    [DataRow(typeof(SecureCoreFeatureIntent))]
    [DataRow(typeof(TorFeatureIntent))]
    public async Task ConnectAsync_ShouldNot_ChangeConnectionIntentWhenPortForwardingDisabledAsync(Type featureIntentType)
    {
        // Arrange
        _settings!.IsPortForwardingEnabled.Returns(false);
        _settings!.VpnPlan.Returns(new VpnPlan(string.Empty, string.Empty, PAID_PLAN_TIER, false));

        ConnectionManager connectionManager = GetConnectionManager();
        IFeatureIntent featureIntent = GetFeatureIntent(featureIntentType);
        IConnectionIntent connectionIntent = GetConnectionIntent(featureIntent);

        // Act
        await connectionManager.ConnectAsync(VpnTriggerDimension.Auto, connectionIntent);

        // Assert
        Assert.IsTrue(connectionManager.CurrentConnectionIntent?.IsSameAs(connectionIntent));
    }

    [TestMethod]
    [DataRow(typeof(SecureCoreFeatureIntent))]
    [DataRow(typeof(TorFeatureIntent))]
    public async Task ReconnectAsync_ShouldNot_ChangeConnectionIntentWhenPortForwardingEnabledAsync(Type featureIntentType)
    {
        // Arrange
        _settings!.IsPortForwardingEnabled.Returns(true);
        _settings!.VpnPlan.Returns(new VpnPlan(string.Empty, string.Empty, PAID_PLAN_TIER, false));

        ConnectionManager connectionManager = GetConnectionManager();
        IFeatureIntent featureIntent = GetFeatureIntent(featureIntentType);
        IConnectionIntent connectionIntent = GetConnectionIntent(featureIntent);

        // Act
        await connectionManager.ConnectAsync(VpnTriggerDimension.Auto, connectionIntent);
        await connectionManager.ReconnectAsync(VpnTriggerDimension.Auto);

        // Assert
        Assert.IsTrue(connectionManager.CurrentConnectionIntent?.IsSameAs(connectionIntent));
    }

    private ConnectionRequestIpcEntity GetConnectionRequestIpcEntity()
    {
        return new ConnectionRequestIpcEntity()
        {
            Servers = [new VpnServerIpcEntity()],
            Credentials = new VpnCredentialsIpcEntity
            {
                Certificate = new ConnectionCertificateIpcEntity()
                {
                    Pem = "pem",
                    ExpirationDateUtc = DateTime.Now.AddDays(1)
                },
                ClientKeyPair = new AsymmetricKeyPairIpcEntity
                {
                    PublicKey = new PublicKeyIpcEntity(),
                    SecretKey = new SecretKeyIpcEntity(),
                }
            }
        };
    }

    private ConnectionManager GetConnectionManager()
    {
        return new(
            _logger!,
            _settings!,
            _vpnServiceCaller!,
            _eventMessageSender!,
            _entityMapper!,
            _connectionRequestCreator!,
            _reconnectionRequestCreator!,
            _disconnectionRequestCreator!,
            _serversLoader!,
            _favoriteServersStorage!,
            _guestHoleServersFileStorage!,
            _guestHoleConnectionRequestCreator!,
            _statisticalEventManager!,
            _connectionKeyManager!,
            _serviceManager!);
    }

    private IConnectionIntent GetConnectionIntent(IFeatureIntent featureIntent)
    {
        return new ConnectionIntent(SingleCountryLocationIntent.From("US"), featureIntent);
    }

    private IFeatureIntent GetFeatureIntent(Type type)
    {
        return (Activator.CreateInstance(type) as IFeatureIntent)!;
    }
}