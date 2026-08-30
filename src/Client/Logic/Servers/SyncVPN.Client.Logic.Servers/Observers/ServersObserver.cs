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

using SyncVPN.Client.Common.Observers;
using SyncVPN.Client.Contracts.Messages;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Logic.Servers.Contracts.Observers;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Configurations.Contracts;
using SyncVPN.IssueReporting.Contracts;
using SyncVPN.Logging.Contracts;

namespace SyncVPN.Client.Logic.Servers.Observers;

public class ServersObserver : PollingObserverBase, IServersObserver,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<DeviceLocationChangedMessage>,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private readonly IServersUpdater _serversUpdater;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IConfiguration _config;

    protected override TimeSpan PollingInterval => TimeSpanExtensions.Min(_config.ServerLoadUpdateInterval, _config.ServerUpdateInterval);

    public ServersObserver(ILogger logger,
        IIssueReporter issueReporter,
        IServersUpdater serversUpdater,
        IUserAuthenticator userAuthenticator,
        IConfiguration config)
        : base(logger, issueReporter)
    {
        _serversUpdater = serversUpdater;
        _userAuthenticator = userAuthenticator;
        _config = config;
    }

    public void Receive(LoggedInMessage message)
    {
        StartTimer();
    }

    public async void Receive(LoggedOutMessage message)
    {
        StopTimer();
        await _serversUpdater.ClearCacheAsync();
    }

    public void Receive(DeviceLocationChangedMessage message)
    {
        TriggerAction.Run();
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        if (message.IsMainWindowVisible)
        {
            TriggerAction.Run();
        }
    }

    protected override async Task OnTriggerAsync()
    {
        if (_userAuthenticator.IsLoggedIn)
        {
            await _serversUpdater.UpdateAsync();
        }
    }
}