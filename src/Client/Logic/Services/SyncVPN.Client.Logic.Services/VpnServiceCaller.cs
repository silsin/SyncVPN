/*
 * Copyright (c) 2023 Proton AG
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

using SyncVPN.Client.Contracts.ProcessCommunication;
using SyncVPN.Client.Logic.Services.Contracts;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Common.Legacy.Abstract;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using SyncVPN.ProcessCommunication.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Controllers;
using SyncVPN.ProcessCommunication.Contracts.Entities.LocalAgent;
using SyncVPN.ProcessCommunication.Contracts.Entities.Settings;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace SyncVPN.Client.Logic.Services;

public class VpnServiceCaller : ServiceCallerBase<IVpnController>, IVpnServiceCaller
{
    public VpnServiceCaller(ILogger logger, IGrpcClient grpcClient,
        Lazy<IServiceCommunicationErrorHandler> serviceCommunicationErrorHandler)
        : base(logger, grpcClient, serviceCommunicationErrorHandler)
    { }

    public async Task ConnectAsync(ConnectionRequestIpcEntity connectionRequest)
    {
        Logger.Info<ProcessCommunicationLog>("[CONNECTION_PROCESS] App -> Service: sending Connect request over IPC.");

        Result<Task> result = await InvokeAsync((c, ct) => c.Connect(connectionRequest, ct).Wrap());

        if (result.Success)
        {
            Logger.Info<ProcessCommunicationLog>("[CONNECTION_PROCESS] App -> Service: Connect request delivered and accepted by the service.");
        }
        else
        {
            Logger.Error<ProcessCommunicationLog>($"[CONNECTION_PROCESS] App -> Service: Connect request could not be delivered to the service. Error: {result.Error}");
        }
    }

    public Task DisconnectAsync(DisconnectionRequestIpcEntity disconnectionRequest)
    {
        return InvokeAsync((c, ct) => c.Disconnect(disconnectionRequest, ct).Wrap());
    }

    public Task<Result<NetworkTrafficIpcEntity>> GetNetworkTrafficAsync()
    {
         return InvokeAsync((c, ct) => c.GetNetworkTraffic(ct));
    }

    public Task RequestNetShieldStatsAsync()
    {
        return InvokeAsync((c, ct) => c.RequestNetShieldStats(ct).Wrap());
    }

    public Task RequestConnectionDetailsAsync()
    {
        return InvokeAsync((c, ct) => c.RequestConnectionDetails(ct).Wrap());
    }

    public Task UpdateLocalAgentTlsCredentialsAsync(LocalAgentTlsCredentialsIpcEntity credentials)
    {
        return InvokeAsync((c, ct) => c.UpdateLocalAgentTlsCredentialsAsync(credentials, ct).Wrap());
    }

    public Task ApplySettingsAsync(MainSettingsIpcEntity settings)
    {
        return InvokeAsync((c, ct) => c.ApplySettings(settings, ct).Wrap());
    }

    public Task RepeatStateAsync()
    {
        return InvokeAsync((c, ct) => c.RepeatState(ct).Wrap());
    }

    public Task RepeatPortForwardingStateAsync()
    {
        return InvokeAsync((c, ct) => c.RepeatPortForwardingState(ct).Wrap());
    }
}