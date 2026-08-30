/*
 * Copyright (c) 2025 Proton AG
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

using SyncVPN.Api.Contracts;
using SyncVPN.Api.Contracts.Auth;
using SyncVPN.Api.Contracts.Common;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Enums;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Users.Contracts;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Configurations.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;

namespace SyncVPN.Client.Logic.Users;

public class VpnPlanUpdater : IVpnPlanUpdater,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    private readonly IApiClient _apiClient;
    private readonly ISettings _settings;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private DateTime _minimumRequestDateUtc = DateTime.MinValue;

    public BaseResponseDetail? AuthResponseDetails { get; private set; }

    public VpnPlanUpdater(IApiClient apiClient,
        ISettings settings,
        ILogger logger,
        IConfiguration configuration,
        IEventMessageSender eventMessageSender)
    {
        _apiClient = apiClient;
        _settings = settings;
        _logger = logger;
        _configuration = configuration;
        _eventMessageSender = eventMessageSender;
    }

    public async Task<VpnPlanChangeResult> ForceUpdateAsync(CancellationToken cancellationToken = default)
    {
        return await EnqueueRequestAsync(isToForceRequest: true, cancellationToken);
    }

    public async Task<VpnPlanChangeResult> UpdateAsync(CancellationToken cancellationToken = default)
    {
        return await EnqueueRequestAsync(isToForceRequest: false, cancellationToken);
    }

    private async Task<VpnPlanChangeResult> EnqueueRequestAsync(bool isToForceRequest, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        ApiResponseResult<VpnInfoWrapperResponse>? response = null;
        VpnPlanChangedMessage? vpnPlanChangedMessage = null;

        try
        {
            if (isToForceRequest || IsToRequest())
            {
                _minimumRequestDateUtc = DateTime.UtcNow + _configuration.VpnPlanMinimumRequestInterval;
                _logger.Info<AppLog>($"Requesting a VPN plan update (Force request: {isToForceRequest}) " +
                    $"(Minimum request date UTC: {_minimumRequestDateUtc})");

                response = await _apiClient.GetVpnInfoResponse(cancellationToken);

                if (response.Success)
                {
                    AuthResponseDetails = null;
                    _settings.MaxDevicesAllowed = response.Value.Vpn.MaxConnect;

                    vpnPlanChangedMessage = GetVpnPlanChangeMessage(response.Value.Vpn);
                    OnResponseSuccess(vpnPlanChangedMessage);
                }
                else
                {
                    AuthResponseDetails = response.Failure && response.Value.Code == ResponseCodes.NO_VPN_CONNECTIONS_ASSIGNED
                        ? response.Value.Details
                        : null;

                    _eventMessageSender.Send<NoVpnConnectionsAssignedMessage>();

                    _logger.Error<AppLog>("VPN plan request failed with " +
                        $"Status Code {response.ResponseMessage.StatusCode}, " +
                        $"Internal Code {response.Value.Code}, " +
                        $"Error '{response.Value.Error}'.");
                }
            }
        }
        catch (Exception e)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            _logger.Error<AppLog>("VPN plan request failed.", e);
        }
        finally
        {
            _semaphore.Release();
        }

        return new VpnPlanChangeResult
        {
            ApiResponse = response,
            PlanChangeMessage = vpnPlanChangedMessage
        };
    }

    private VpnPlanChangedMessage GetVpnPlanChangeMessage(VpnInfoResponse vpnInfoResponse)
    {
        VpnPlan oldPlan = _settings.VpnPlan;
        VpnPlan newPlan = new(vpnInfoResponse.PlanTitle, vpnInfoResponse.PlanName, vpnInfoResponse.MaxTier, vpnInfoResponse.IsBusiness);
        return new(oldPlan: oldPlan, newPlan: newPlan);
    }

    private bool IsToRequest()
    {
        return DateTime.UtcNow >= _minimumRequestDateUtc;
    }

    private void OnResponseSuccess(VpnPlanChangedMessage message)
    {
        if (message.HasChanged())
        {
            _settings.VpnPlan = message.NewPlan;
            _eventMessageSender.Send(message);
        }
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        if (message.AuthenticationStatus is AuthenticationStatus.LoggingOut or AuthenticationStatus.LoggedOut)
        {
            AuthResponseDetails = null;
        }
    }
}