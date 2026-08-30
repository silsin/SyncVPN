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

using SyncVPN.Client.Common.Enums;
using SyncVPN.Client.Contracts.Services.Browsing;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Handlers.Bases;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Logic.Profiles.Contracts.Models;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;

namespace SyncVPN.Client.Handlers;

public class ProfileConnectAndGoHandler : IHandler,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private const int DELAY_AFTER_CONNECTION_IN_MS = 1000;

    private readonly IConnectionManager _connectionManager;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IFilesBrowser _appsBrowser;
    private readonly ILogger _logger;

    private IConnectionProfile? _lastProfile;

    public ProfileConnectAndGoHandler(
        IConnectionManager connectionManager,
        IUrlsBrowser urlsBrowser,
        IFilesBrowser appsBrowser,
        ILogger logger)
    {
        _connectionManager = connectionManager;
        _urlsBrowser = urlsBrowser;
        _appsBrowser = appsBrowser;
        _logger = logger;
    }

    public async void Receive(ConnectionStatusChangedMessage message)
    {
        if (_connectionManager.IsDisconnected)
        {
            _lastProfile = null;
            return;
        }

        if (_connectionManager.IsConnected &&
            _connectionManager.CurrentConnectionIntent is IConnectionProfile profile &&
            profile.Options.ConnectAndGo.IsEnabled)
        {
            // Extra delay to ensure the connection is fully established before triggering Connect and go.
            await Task.Delay(DELAY_AFTER_CONNECTION_IN_MS);

            if (_lastProfile != null && _lastProfile.IsSameAs(profile))
            {
                return;
            }

            _lastProfile = profile;

            IConnectAndGoOption connectAndGo = profile.Options.ConnectAndGo;

            switch (connectAndGo.Mode)
            {
                case ConnectAndGoMode.Website:
                    string url = connectAndGo.Url.ToFormattedUrl();
                    _logger.Info<AppLog>($"Connect and go - Open a website: {url}");
                    _urlsBrowser.BrowseTo(url, connectAndGo.UsePrivateBrowsingMode);
                    break;

                case ConnectAndGoMode.Application:
                    string appPath = connectAndGo.AppPath ?? string.Empty;
                    _logger.Info<AppLog>($"Connect and go - Open an app: {appPath}");
                    _appsBrowser.OpenApp(appPath);
                    break;
            }
        }
    }
}