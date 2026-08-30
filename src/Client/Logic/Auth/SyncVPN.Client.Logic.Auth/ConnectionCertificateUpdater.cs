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

using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Configurations.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.UserCertificateLogs;

namespace SyncVPN.Client.Logic.Auth;

public class ConnectionCertificateUpdater : IConnectionCertificateUpdater,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggingOutMessage>
{
    private readonly IConfiguration _config;
    private readonly IConnectionCertificateManager _connectionCertificateManager;
    private readonly ILogger _logger;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private Timer? _timer;

    public ConnectionCertificateUpdater(IConfiguration config,
        IConnectionCertificateManager connectionCertificateManager,
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _config = config;
        _connectionCertificateManager = connectionCertificateManager;
        _logger = logger;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    private void Timer_OnTick(object? sender)
    {
        // TODO: Does this need to be done on the UI thread?
        _uiThreadDispatcher.TryEnqueue(() => _connectionCertificateManager.RequestNewCertificateAsync());
    }

    public void Receive(LoggedInMessage message)
    {
        TimeSpan interval = _config.ConnectionCertificateUpdateInterval;
        _timer = new(Timer_OnTick);
        _timer.Change(interval, interval);
        _logger.Info<UserCertificateScheduleRefreshLog>(
            $"Connection certificate refresh scheduled for every '{interval}'.");
    }

    public void Receive(LoggingOutMessage message)
    {
        _timer?.Dispose();
    }
}