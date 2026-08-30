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

using SyncVPN.Client.Contracts.Messages;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Handlers.Bases;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Services.Contracts;
using SyncVPN.Client.Logic.Users.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;
using SyncVPN.OperatingSystems.PowerEvents.Contracts;

namespace SyncVPN.Client.Handlers;

public class PowerModeAndWindowActivationHandler : IHandler,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private readonly ILogger _logger;
    private readonly ISettings _settings;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly IConnectionCertificateManager _connectionCertificateManager;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IVpnPlanUpdater _vpnPlanUpdater;

    public PowerModeAndWindowActivationHandler(ILogger logger,
        ISettings settings,
        IVpnServiceCaller vpnServiceCaller,
        IConnectionCertificateManager connectionCertificateManager,
        IUserAuthenticator userAuthenticator,
        IPowerEventNotifier powerEventNotifier,
        IVpnPlanUpdater vpnPlanUpdater)
    {
        _logger = logger;
        _settings = settings;
        _vpnServiceCaller = vpnServiceCaller;
        _connectionCertificateManager = connectionCertificateManager;
        _userAuthenticator = userAuthenticator;
        _vpnPlanUpdater = vpnPlanUpdater;

        powerEventNotifier.OnResume += OnResume;
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        if (!_userAuthenticator.IsLoggedIn)
        {
            return;
        }

        if (message.IsMainWindowVisible)
        {
            _logger.Debug<AppLog>("Main window activated while logged in");
            OnResumeOrWindowActivation();
        }
    }

    private void OnResume(object? sender, EventArgs e)
    {
        if (_userAuthenticator.IsLoggedIn)
        {
            _logger.Info<AppLog>("Resuming from sleep while logged in");
            OnResumeOrWindowActivation();
        }
        else
        {
            _logger.Info<AppLog>("Resuming from sleep while logged out");
        }
    }

    private void OnResumeOrWindowActivation()
    {
        _vpnServiceCaller.RepeatStateAsync();
        if (_settings.IsPortForwardingEnabled)
        {
            _vpnServiceCaller.RepeatPortForwardingStateAsync();
        }

        _connectionCertificateManager.RequestNewCertificateAsync();
        _vpnPlanUpdater.UpdateAsync();
    }
}