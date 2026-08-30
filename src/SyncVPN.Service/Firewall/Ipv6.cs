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

using System;
using System.Threading.Tasks;
using SyncVPN.Configurations.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.NetworkLogs;
using SyncVPN.OperatingSystems.Network.Contracts;
using SyncVPN.Service.Settings;

namespace SyncVPN.Service.Firewall;

internal class Ipv6 : IIpv6
{
    private const string APP_NAME = "SyncVPN";

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfig;
    private readonly IServiceSettings _serviceSettings;
    private readonly INetworkUtilities _networkUtilities;

    public Ipv6(
        ILogger logger,
        IStaticConfiguration staticConfig,
        IServiceSettings serviceSettings,
        INetworkUtilities networkUtilities)
    {
        _logger = logger;
        _staticConfig = staticConfig;
        _serviceSettings = serviceSettings;
        _networkUtilities = networkUtilities;
    }

    public bool IsEnabled { get; private set; } = true;

    public Task DisableAsync()
    {
        return Task.Run(Disable);
    }

    public Task EnableAsync()
    {
        return Task.Run(Enable);
    }

    public Task EnableOnVPNInterfaceAsync()
    {
        return Task.Run(EnableOnVPNInterface);
    }

    public void Enable()
    {
        if (LoggingAction(_networkUtilities.EnableIPv6OnAllAdapters, "Enabling"))
        {
            IsEnabled = true;
        }
    }

    private void Disable()
    {
        if (LoggingAction(_networkUtilities.DisableIPv6OnAllAdapters, "Disabling"))
        {
            IsEnabled = false;
        }
    }

    private void EnableOnVPNInterface()
    {
        LoggingAction(_networkUtilities.EnableIPv6, "Enabling on VPN interface");
    }

    private bool LoggingAction(Action<string, string> action, string actionMessage)
    {
        try
        {
            _logger.Info<NetworkLog>($"IPv6: {actionMessage}");
            action(APP_NAME, _staticConfig.GetHardwareId(_serviceSettings.OpenVpnAdapter));
            _logger.Info<NetworkLog>($"IPv6: {actionMessage} succeeded");

            return true;
        }
        catch (NetworkUtilException e)
        {
            _logger.Error<NetworkLog>($"IPV6: {actionMessage} failed, error code {e.Code}");

            return false;
        }
    }
}