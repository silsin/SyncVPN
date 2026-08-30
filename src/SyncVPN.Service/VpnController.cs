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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SyncVPN.Common.Core.Helpers;
using SyncVPN.Common.Core.Networking;
using SyncVPN.Common.Legacy.Threading;
using SyncVPN.Common.Legacy.Vpn;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.ConnectLogs;
using SyncVPN.Logging.Contracts.Events.DisconnectLogs;
using SyncVPN.ProcessCommunication.Contracts.Controllers;
using SyncVPN.ProcessCommunication.Contracts.Entities.LocalAgent;
using SyncVPN.ProcessCommunication.Contracts.Entities.Settings;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;
using SyncVPN.Service.ControllerRetries;
using SyncVPN.Service.ProcessCommunication;
using SyncVPN.Service.Settings;
using SyncVPN.Vpn.Common;
using SyncVPN.Vpn.LocalAgent;
using SyncVPN.Vpn.PortMapping;

namespace SyncVPN.Service;

public class VpnController : IVpnController
{
    private readonly IVpnConnection _vpnConnection;
    private readonly ILogger _logger;
    private readonly IServiceSettings _serviceSettings;
    private readonly ITaskQueue _taskQueue;
    private readonly IPortMappingProtocolClient _portMappingProtocolClient;
    private readonly IClientControllerSender _appControllerCaller;
    private readonly IEntityMapper _entityMapper;
    private readonly ILocalAgentTlsCredentialsCache _localAgentTlsCredentialsCache;
    private readonly IControllerRetryManager _controllerRetryManager;

    public VpnController(
        IVpnConnection vpnConnection,
        ILogger logger,
        IServiceSettings serviceSettings,
        ITaskQueue taskQueue,
        IPortMappingProtocolClient portMappingProtocolClient,
        IClientControllerSender appControllerCaller,
        IEntityMapper entityMapper,
        ILocalAgentTlsCredentialsCache localAgentTlsCredentialsCache,
        IControllerRetryManager controllerRetryManager)
    {
        _vpnConnection = vpnConnection;
        _logger = logger;
        _serviceSettings = serviceSettings;
        _taskQueue = taskQueue;
        _portMappingProtocolClient = portMappingProtocolClient;
        _appControllerCaller = appControllerCaller;
        _entityMapper = entityMapper;
        _localAgentTlsCredentialsCache = localAgentTlsCredentialsCache;
        _controllerRetryManager = controllerRetryManager;
    }

    public async Task Connect(ConnectionRequestIpcEntity connectionRequest, CancellationToken cancelToken)
    {
        Ensure.NotNull(connectionRequest, nameof(connectionRequest));
        _controllerRetryManager.EnforceRetryId(connectionRequest);

        _logger.Info<ConnectLog>("Connect requested");

        _serviceSettings.Apply(connectionRequest.Settings);

        VpnConfig config = _entityMapper.Map<VpnConfigIpcEntity, VpnConfig>(connectionRequest.Config);
        config.OpenVpnAdapter = _serviceSettings.OpenVpnAdapter;
        IReadOnlyList<VpnHost> endpoints = _entityMapper.Map<VpnServerIpcEntity, VpnHost>(connectionRequest.Servers);
        VpnCredentials credentials = _entityMapper.Map<VpnCredentialsIpcEntity, VpnCredentials>(connectionRequest.Credentials);
        _localAgentTlsCredentialsCache.Set(new LocalAgentTlsCredentials(
            new ConnectionCertificate(credentials.ClientCertPem, credentials.ClientCertificateExpirationDateUtc),
            credentials.ClientKeyPair));
        _vpnConnection.Connect(endpoints, config, credentials);
    }

    public async Task Disconnect(DisconnectionRequestIpcEntity disconnectionRequest, CancellationToken cancelToken)
    {
        Ensure.NotNull(disconnectionRequest, nameof(disconnectionRequest));
        _controllerRetryManager.EnforceRetryId(disconnectionRequest);

        _logger.Info<DisconnectLog>($"Disconnect requested (Error: {disconnectionRequest.ErrorType})");
        _serviceSettings.Apply(disconnectionRequest.Settings);
        _vpnConnection.Disconnect((VpnError)disconnectionRequest.ErrorType);
    }

    public async Task UpdateLocalAgentTlsCredentialsAsync(LocalAgentTlsCredentialsIpcEntity credentialsIpcEntity, CancellationToken cancelToken)
    {
        LocalAgentTlsCredentials credentials = _entityMapper.Map<LocalAgentTlsCredentialsIpcEntity, LocalAgentTlsCredentials>(credentialsIpcEntity);
        _localAgentTlsCredentialsCache.Set(credentials);
    }

    public async Task<NetworkTrafficIpcEntity> GetNetworkTraffic(CancellationToken cancelToken)
    {
        return _entityMapper.Map<NetworkTraffic, NetworkTrafficIpcEntity>(_vpnConnection.NetworkTraffic);
    }

    public async Task ApplySettings(MainSettingsIpcEntity settings, CancellationToken cancelToken)
    {
        Ensure.NotNull(settings, nameof(settings));
        _serviceSettings.Apply(settings);
    }

    public async Task RepeatState(CancellationToken cancelToken)
    {
        _taskQueue.Enqueue(async () =>
        {
            await _appControllerCaller.SendCurrentVpnStateAsync();
        });
    }

    public async Task RepeatPortForwardingState(CancellationToken cancelToken)
    {
        _portMappingProtocolClient.RepeatState();
    }

    public async Task RequestNetShieldStats(CancellationToken cancelToken)
    {
        _vpnConnection.RequestNetShieldStats();
    }

    public async Task RequestConnectionDetails(CancellationToken cancelToken)
    {
        _vpnConnection.RequestConnectionDetails();
    }
}