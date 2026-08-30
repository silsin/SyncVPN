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

using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
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

    public ConnectionRequestCreator(
        ILogger logger,
        ISettings settings,
        IEntityMapper entityMapper,
        IConnectionKeyManager connectionKeyManager,
        IConnectionCertificateManager connectionCertificateManager,
        IServerListGenerator serverListGenerator,
        ISmartServerListGenerator smartServerListGenerator,
        IFeatureFlagsObserver featureFlagsObserver,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
        : base(logger, settings, entityMapper, featureFlagsObserver, mainSettingsRequestCreator)
    {
        ServerListGenerator = serverListGenerator;
        SmartServerListGenerator = smartServerListGenerator;

        _connectionKeyManager = connectionKeyManager;
        _connectionCertificateManager = connectionCertificateManager;
    }

    public virtual async Task<ConnectionRequestIpcEntity> CreateAsync(IConnectionIntent connectionIntent)
    {
        MainSettingsIpcEntity settings = GetSettings(connectionIntent);
        VpnConfigIpcEntity config = GetVpnConfig(settings, connectionIntent);
        List<VpnProtocol> preferredProtocols = EntityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(config.PreferredProtocols);
        ServerListResult serverListResult = GetServerListResult(connectionIntent, preferredProtocols);
        VpnServerIpcEntity[] servers = PhysicalServersToVpnServerIpcEntities(serverListResult.PhysicalServers);
        bool areAllServersExcluded = servers.Length == 0 && serverListResult.Diagnostic.AreAllCandidatesExcluded;

        ConnectionRequestIpcEntity request = new()
        {
            RetryId = Guid.NewGuid(),
            Config = config,
            Credentials = await GetVpnCredentialsAsync(),
            Protocol = settings.VpnProtocol,
            Servers = servers,
            Settings = settings,
            AreAllServersExcludedByUserPreference = areAllServersExcluded,
        };

        return request;
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