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
using SyncVPN.Api.V2.Contracts;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.Client.Logic.Connection.Contracts.RequestCreators;
using SyncVPN.Client.Logic.Connection.Contracts.ServerListGenerators;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Observers;
using SyncVPN.Common.Core.Networking;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Settings;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace SyncVPN.Client.Logic.Connection.RequestCreators;

public class ReconnectionRequestCreator : ConnectionRequestCreator, IReconnectionRequestCreator
{
    public ReconnectionRequestCreator(
        ISettings settings,
        ILogger logger,
        IEntityMapper entityMapper,
        IConnectionKeyManager connectionKeyManager,
        IConnectionCertificateManager connectionCertificateManager,
        IServerListGenerator serverListGenerator,
        ISmartServerListGenerator smartServerListGenerator,
        IFeatureFlagsObserver featureFlagsObserver,
        IMainSettingsRequestCreator mainSettingsRequestCreator,
        IBackendModeProvider backendModeProvider,
        ISyncVpnApiClient syncVpnApiClient,
        IEventMessageSender eventMessageSender,
        Lazy<IUserAuthenticator> userAuthenticator)
        : base(logger,
               settings,
               entityMapper,
               connectionKeyManager,
               connectionCertificateManager,
               serverListGenerator,
               smartServerListGenerator,
               featureFlagsObserver,
               mainSettingsRequestCreator,
               backendModeProvider,
               syncVpnApiClient,
               eventMessageSender,
               userAuthenticator)
    { }

    public override async Task<ConnectionRequestIpcEntity> CreateAsync(IConnectionIntent connectionIntent)
    {
        MainSettingsIpcEntity settings = GetSettings(connectionIntent);
        VpnConfigIpcEntity config = GetVpnConfig(settings, connectionIntent);

        // If the protocol in the settings is a specific one (not Smart), put it at the top of the smart protocol list
        if (settings.VpnProtocol != VpnProtocolIpcEntity.Smart)
        {
            IList<VpnProtocolIpcEntity> smartProtocols = GetPreferredSmartProtocols();
            smartProtocols.Remove(settings.VpnProtocol);
            // Insert even if the protocol doesn't exist in the smart protocol list (ex.: Countries with only Stealth)
            smartProtocols.Insert(0, settings.VpnProtocol);
            config.PreferredProtocols = smartProtocols;
        }

        List<VpnProtocol> preferredProtocols = EntityMapper.Map<VpnProtocolIpcEntity, VpnProtocol>(config.PreferredProtocols);
        ServerListResult serverListResult = GetReconnectionServerListResult(connectionIntent, preferredProtocols);
        (VpnServerIpcEntity[] servers, VpnCredentialsIpcEntity credentials, int? sstpPort) =
            await ResolveServersAndCredentialsAsync(connectionIntent, serverListResult.PhysicalServers, preferredProtocols);
        ApplyClaimedSstpPort(config, sstpPort);
        bool areAllServersExcluded = servers.Length == 0 && serverListResult.Diagnostic.AreAllCandidatesExcluded;

        ConnectionRequestIpcEntity request = new()
        {
            RetryId = Guid.NewGuid(),
            Config = config,
            Credentials = credentials,
            Protocol = VpnProtocolIpcEntity.Smart,
            Servers = servers,
            Settings = settings,
            AreAllServersExcludedByUserPreference = areAllServersExcluded,
        };

        return request;
    }

    private ServerListResult GetReconnectionServerListResult(IConnectionIntent connectionIntent, IList<VpnProtocol> preferredProtocols)
    {
        ServerListResult intentResult = ServerListGenerator.Generate(connectionIntent, preferredProtocols);

        if (IsToBypassSmartServerListGenerator(connectionIntent))
        {
            return intentResult;
        }

        ServerListResult smartResult = SmartServerListGenerator.Generate(connectionIntent, preferredProtocols);

        // Merge physical servers, avoiding duplicates by Id
        List<PhysicalServer> mergedServers = smartResult.PhysicalServers.ToList();
        mergedServers.AddRange(intentResult.PhysicalServers.Where(ips => !mergedServers.Any(slps => slps.Id == ips.Id)));

        // Combine both diagnostics together, considering all candidates are excluded if both generators excluded them all
        ServerListDiagnostic mergedDiagnostic = new ServerListDiagnostic(
            intentResult.Diagnostic.AreAllCandidatesExcluded && smartResult.Diagnostic.AreAllCandidatesExcluded);

        return new ServerListResult(mergedServers, mergedDiagnostic);
    }
}