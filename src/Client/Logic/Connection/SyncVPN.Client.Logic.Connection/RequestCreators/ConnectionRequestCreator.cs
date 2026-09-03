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

using SyncVPN.Api.BackendSelection;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts;
using SyncVPN.Api.V2.Contracts.Account;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.SyncVpnServers;
using SyncVPN.Client.Logic.Connection.Contracts.RequestCreators;
using SyncVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Observers;
using SyncVPN.Common.Core.Networking;
using SyncVPN.Common.Legacy.Vpn;
using SyncVPN.Crypto.Contracts;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.UserCertificateLogs;
using SyncVPN.ProcessCommunication.Contracts.Entities.Crypto;
using SyncVPN.ProcessCommunication.Contracts.Entities.LocalAgent;
using SyncVPN.ProcessCommunication.Contracts.Entities.Settings;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace SyncVPN.Client.Logic.Connection.RequestCreators;

public class ConnectionRequestCreator : ConnectionRequestCreatorBase, IConnectionRequestCreator
{
    protected readonly IServerListGenerator ServerListGenerator;
    protected readonly ISmartServerListGenerator SmartServerListGenerator;

    private readonly IConnectionKeyManager _connectionKeyManager;
    private readonly IConnectionCertificateManager _connectionCertificateManager;
    private readonly IBackendModeProvider _backendModeProvider;
    private readonly ISyncVpnApiClient _syncVpnApiClient;
    private readonly Lazy<IUserAuthenticator> _userAuthenticator;

    public ConnectionRequestCreator(
        ILogger logger,
        ISettings settings,
        IEntityMapper entityMapper,
        IConnectionKeyManager connectionKeyManager,
        IConnectionCertificateManager connectionCertificateManager,
        IServerListGenerator serverListGenerator,
        ISmartServerListGenerator smartServerListGenerator,
        IFeatureFlagsObserver featureFlagsObserver,
        IMainSettingsRequestCreator mainSettingsRequestCreator,
        IBackendModeProvider backendModeProvider,
        ISyncVpnApiClient syncVpnApiClient,
        Lazy<IUserAuthenticator> userAuthenticator)
        : base(logger, settings, entityMapper, featureFlagsObserver, mainSettingsRequestCreator)
    {
        ServerListGenerator = serverListGenerator;
        SmartServerListGenerator = smartServerListGenerator;

        _connectionKeyManager = connectionKeyManager;
        _connectionCertificateManager = connectionCertificateManager;
        _backendModeProvider = backendModeProvider;
        _syncVpnApiClient = syncVpnApiClient;
        _userAuthenticator = userAuthenticator;
    }

    // A device-registered guest has no legacy Proton session/certificate to request credentials from,
    // so it always claims via the new backend's POST /account regardless of the global VpnProvisioning
    // rollout flag - that flag only governs whether a *logged-in* Proton user's connections switch over,
    // which is a separate, higher-stakes rollout decision left untouched here.
    // IUserAuthenticator is injected lazily: UserAuthenticator -> GuestHoleManager -> ConnectionManager
    // -> ConnectionRequestCreator already forms a cycle back to this class, so an eager dependency here
    // would make Autofac fail to resolve the graph at all. Lazy<T> defers resolution past container
    // build time, by which point the cycle is a non-issue (both sides already exist as singletons).
    private bool IsVpnProvisioningEnabled =>
        _backendModeProvider.IsNewBackendEnabled(BackendCapability.VpnProvisioning) || !_userAuthenticator.Value.IsLoggedIn;

    public virtual async Task<ConnectionRequestIpcEntity> CreateAsync(IConnectionIntent connectionIntent)
    {
        MainSettingsIpcEntity settings = GetSettings(connectionIntent);
        VpnConfigIpcEntity config = GetVpnConfig(settings, connectionIntent);
        List<VpnProtocol> preferredProtocols = EntityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(config.PreferredProtocols);
        ServerListResult serverListResult = GetServerListResult(connectionIntent, preferredProtocols);
        (VpnServerIpcEntity[] servers, VpnCredentialsIpcEntity credentials) =
            await ResolveServersAndCredentialsAsync(connectionIntent, serverListResult.PhysicalServers, preferredProtocols);
        bool areAllServersExcluded = servers.Length == 0 && serverListResult.Diagnostic.AreAllCandidatesExcluded;

        ConnectionRequestIpcEntity request = new()
        {
            RetryId = Guid.NewGuid(),
            Config = config,
            Credentials = credentials,
            Protocol = settings.VpnProtocol,
            Servers = servers,
            Settings = settings,
            AreAllServersExcludedByUserPreference = areAllServersExcluded,
        };

        return request;
    }

    // Shared by ConnectionRequestCreator and ReconnectionRequestCreator: on the legacy Proton path,
    // servers/credentials come from two independent sources (the cached server catalog + a certificate
    // request). On the new SyncVPN backend, a single POST /account call returns both at once for a
    // chosen candidate server - see the migration plan's VpnProvisioning phase.
    protected async Task<(VpnServerIpcEntity[] Servers, VpnCredentialsIpcEntity Credentials)> ResolveServersAndCredentialsAsync(
        IConnectionIntent connectionIntent, IReadOnlyList<PhysicalServer> candidates, IList<VpnProtocol> preferredProtocols)
    {
        // A specific new-backend server was already chosen (e.g. from the Free Servers list) - claim
        // it directly rather than picking a candidate from the legacy Proton cache, which doesn't
        // contain it at all.
        if (connectionIntent.Location is SyncVpnServerLocationIntent syncVpnServerIntent)
        {
            return await ClaimAccountAsync(syncVpnServerIntent);
        }

        if (IsVpnProvisioningEnabled)
        {
            return await ClaimAccountAsync(candidates, preferredProtocols);
        }

        return (PhysicalServersToVpnServerIpcEntities(candidates), await GetVpnCredentialsAsync());
    }

    private async Task<(VpnServerIpcEntity[] Servers, VpnCredentialsIpcEntity Credentials)> ClaimAccountAsync(
        SyncVpnServerLocationIntent intent)
    {
        ApiResponseResult<ClaimAccountResponse> response = await _syncVpnApiClient.ClaimAccountAsync(
            new ClaimAccountRequest { ServerId = intent.ServerId, Protocol = intent.Protocol, Transport = intent.Transport });

        if (response.Failure || response.Value is null)
        {
            throw new InvalidOperationException($"Failed to claim a SyncVPN account: {response.Error}");
        }

        PurchasedAccount account = response.Value.Data.Account;
        return (SyncVpnAccountClaimMapper.BuildClaimedServer(account), SyncVpnAccountClaimMapper.BuildClaimedCredentials(account));
    }

    private async Task<(VpnServerIpcEntity[] Servers, VpnCredentialsIpcEntity Credentials)> ClaimAccountAsync(
        IReadOnlyList<PhysicalServer> candidates, IList<VpnProtocol> preferredProtocols)
    {
        if (candidates.Count == 0 || !long.TryParse(candidates[0].Id, out long serverId))
        {
            throw new InvalidOperationException("Cannot claim a SyncVPN account: no candidate server available.");
        }

        (string protocol, string? transport) = SyncVpnAccountClaimMapper.ResolveClaimProtocol(preferredProtocols);

        ApiResponseResult<ClaimAccountResponse> response = await _syncVpnApiClient.ClaimAccountAsync(
            new ClaimAccountRequest { ServerId = serverId, Protocol = protocol, Transport = transport });

        if (response.Failure || response.Value is null)
        {
            throw new InvalidOperationException($"Failed to claim a SyncVPN account: {response.Error}");
        }

        PurchasedAccount account = response.Value.Data.Account;
        return (SyncVpnAccountClaimMapper.BuildClaimedServer(account), SyncVpnAccountClaimMapper.BuildClaimedCredentials(account));
    }

    protected ServerListResult GetServerListResult(IConnectionIntent connectionIntent, IList<VpnProtocol> preferredProtocols)
    {
        return IsToBypassSmartServerListGenerator(connectionIntent)
            ? ServerListGenerator.Generate(connectionIntent, preferredProtocols)
            : SmartServerListGenerator.Generate(connectionIntent, preferredProtocols);
    }

    protected override async Task<VpnCredentialsIpcEntity> GetVpnCredentialsAsync()
    {
        await RequestCertificateIfNecessaryAsync();

        ConnectionCertificate? connectionCertificate = Settings.ConnectionCertificate;
        AsymmetricKeyPair? keyPair = _connectionKeyManager.GetKeyPairOrNull();

        return new VpnCredentialsIpcEntity
        {
            Certificate = connectionCertificate.HasValue ? CreateCertificate(connectionCertificate.Value) : null,
            ClientKeyPair = keyPair is null ? null : new AsymmetricKeyPairIpcEntity
            {
                PublicKey = EntityMapper.Map<PublicKey, PublicKeyIpcEntity>(keyPair.PublicKey),
                SecretKey = EntityMapper.Map<SecretKey, SecretKeyIpcEntity>(keyPair.SecretKey)
            }
        };
    }

    private async Task RequestCertificateIfNecessaryAsync()
    {
        if (_connectionKeyManager.GetKeyPairOrNull() is null)
        {
            Logger.Info<UserCertificateLog>("Connection keys are missing, forcing new keys and certificate.");
            await _connectionCertificateManager.ForceRequestNewKeyPairAndCertificateAsync();
        }
        else
        {
            ConnectionCertificate? connectionCertificate = Settings.ConnectionCertificate;

            if (connectionCertificate is null)
            {
                Logger.Info<UserCertificateLog>("Connection certificate is missing, requesting a new certificate.");
                await _connectionCertificateManager.RequestNewCertificateAsync();
            }
            else if (connectionCertificate.Value.ExpirationUtcDate <= DateTimeOffset.UtcNow)
            {
                Logger.Info<UserCertificateLog>("Connection certificate is expired, requesting a new certificate.");
                await _connectionCertificateManager.RequestNewCertificateAsync();
            }
        }
    }

    private ConnectionCertificateIpcEntity CreateCertificate(ConnectionCertificate connectionCertificate)
    {
        return new()
        {
            Pem = connectionCertificate.Pem,
            ExpirationDateUtc = connectionCertificate.ExpirationUtcDate.UtcDateTime,
        };
    }

    protected VpnServerIpcEntity[] PhysicalServersToVpnServerIpcEntities(IEnumerable<PhysicalServer> physicalServers)
    {
        IEnumerable<VpnHost> hosts = physicalServers
            .Select(s => new VpnHost(s.Domain, s.EntryIp, s.Label, GetServerPublicKey(s), s.Signature, s.IsIpv6Supported, s.RelayIpByProtocol));
        return EntityMapper.Map<VpnHost, VpnServerIpcEntity>(hosts).ToArray();
    }

    protected PublicKey? GetServerPublicKey(PhysicalServer server)
    {
        return string.IsNullOrEmpty(server.X25519PublicKey)
            ? null
            : new PublicKey(server.X25519PublicKey, KeyAlgorithm.X25519);
    }
}