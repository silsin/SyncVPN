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
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Users.Contracts;
using SyncVPN.Configurations.Contracts;
using SyncVPN.IssueReporting.Contracts;
using SyncVPN.Logging.Contracts;

namespace SyncVPN.Client.Logic.Users.Observers;

public class VpnPlanObserver : PollingObserverBase, IObserver,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>
{
    private readonly IVpnPlanUpdater _vpnPlanUpdater;
    private readonly IConfiguration _config;

    protected override TimeSpan PollingInterval => _config.VpnPlanRequestInterval;

    public VpnPlanObserver(ILogger logger,
        IIssueReporter issueReporter,
        IVpnPlanUpdater vpnPlanUpdater,
        IConfiguration config)
        : base(logger, issueReporter)
    {
        _vpnPlanUpdater = vpnPlanUpdater;
        _config = config;
    }

    public void Receive(LoggedInMessage message)
    {
        StartTimer();
    }

    public void Receive(LoggedOutMessage message)
    {
        StopTimer();
    }

    protected override async Task OnTriggerAsync()
    {
        await _vpnPlanUpdater.UpdateAsync();
    }
}