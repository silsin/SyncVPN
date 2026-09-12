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

using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts;
using SyncVPN.Api.V2.Contracts.Account;
using SyncVPN.Client.Common.Observers;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.History;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Common.Core.Networking;
using SyncVPN.IssueReporting.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;

namespace SyncVPN.Client.Logic.Connection;

// Periodically reports accrued VPN traffic to the new SyncVPN backend's POST /account/usage while
// connected, and flushes whatever's left over on disconnect - the backend accumulates these deltas onto
// the account's real running totals, which UsageReportingObserver then stores back into settings for the
// sidebar's free-plan card to show (see SidebarComponentViewModel), replacing what used to be a static
// placeholder ("512 MB / 512 MB") with the account's actual usage.
public class UsageReportingObserver : PollingObserverBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private static readonly TimeSpan ReportingInterval = TimeSpan.FromSeconds(60);

    private readonly IConnectionManager _connectionManager;
    private readonly INetworkTrafficManager _networkTrafficManager;
    private readonly ISyncVpnApiClient _syncVpnApiClient;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;

    private NetworkTraffic _lastReportedVolume = NetworkTraffic.Zero;

    protected override TimeSpan PollingInterval => ReportingInterval;

    public UsageReportingObserver(
        IConnectionManager connectionManager,
        INetworkTrafficManager networkTrafficManager,
        ISyncVpnApiClient syncVpnApiClient,
        ISettings settings,
        IEventMessageSender eventMessageSender,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(logger, issueReporter)
    {
        _connectionManager = connectionManager;
        _networkTrafficManager = networkTrafficManager;
        _syncVpnApiClient = syncVpnApiClient;
        _settings = settings;
        _eventMessageSender = eventMessageSender;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (_connectionManager.IsConnected)
        {
            _lastReportedVolume = NetworkTraffic.Zero;
            StartTimer();
        }
        else
        {
            StopTimer();

            // Flush whatever accrued since the last periodic report before the session's volume resets.
            _ = ReportUsageAsync();
        }
    }

    protected override async Task OnTriggerAsync()
    {
        if (!_connectionManager.IsConnected)
        {
            return;
        }

        await ReportUsageAsync();
    }

    private async Task ReportUsageAsync()
    {
        NetworkTraffic currentVolume = _networkTrafficManager.GetVolume();
        NetworkTraffic delta = currentVolume - _lastReportedVolume;

        if (delta.BytesUploaded == 0 && delta.BytesDownloaded == 0)
        {
            return;
        }

        UsageReportRequest request = new()
        {
            ReportId = Guid.NewGuid(),
            SentMbDelta = BytesToMb(delta.BytesUploaded),
            ReceivedMbDelta = BytesToMb(delta.BytesDownloaded),
        };

        ApiResponseResult<UsageReportResponse> response = await _syncVpnApiClient.ReportUsageAsync(request);

        if (response.Success && response.Value is not null)
        {
            _lastReportedVolume = currentVolume;

            _settings.SyncVpnAccountSentMb = response.Value.Data.Account.Usage.SentMb;
            _settings.SyncVpnAccountReceivedMb = response.Value.Data.Account.Usage.ReceivedMb;

            _eventMessageSender.Send<AccountUsageChangedMessage>();
        }
        else
        {
            Logger.Error<AppLog>($"Failed to report account usage: {response.Error}");
        }
    }

    private static double BytesToMb(ulong bytes)
    {
        return bytes / (1024d * 1024d);
    }
}
