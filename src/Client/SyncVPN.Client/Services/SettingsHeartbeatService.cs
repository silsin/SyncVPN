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

using SyncVPN.Client.Common.Observers;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.IssueReporting.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;
using SyncVPN.StatisticalEvents.Contracts;
using SyncVPN.Common.Core.Extensions;

namespace SyncVPN.Client.Services;

public class SettingsHeartbeatService : PollingObserverBase,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>
{
    private readonly static TimeSpan HEARTBEAT_MINIMUM_DELAY = TimeSpan.FromMinutes(3);
    private readonly static TimeSpan HEARTBEAT_INTERVAL = TimeSpan.FromHours(24);

    private readonly ISettingsHeartbeatReporter _settingsHeartbeatReporter;
    private readonly ISettings _settings;

    protected override TimeSpan PollingInterval { get; } = HEARTBEAT_INTERVAL;
    protected override TimeSpan? InitialDelay => CalculateInitialDelay();

    public SettingsHeartbeatService(
        ISettingsHeartbeatReporter settingsHeartbeatReporter,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings)
        : base(logger, issueReporter)
    {
        _settingsHeartbeatReporter = settingsHeartbeatReporter;
        _settings = settings;
    }

    public void Receive(LoggedInMessage message)
    {
        try
        {
            if (_settings.LastSettingsHeartbeatTimeUtc == null)
            {
                _settings.LastSettingsHeartbeatTimeUtc = DateTimeOffset.UtcNow;
            }

            Start();
        }
        catch (Exception ex)
        {
            Logger.Error<AppLog>("Failed to start settings heartbeat service", ex);
        }
    }

    public void Receive(LoggedOutMessage message)
    {
        try
        {
            Stop();
        }
        catch (Exception ex)
        {
            Logger.Error<AppLog>("Failed to stop settings heartbeat service", ex);
        }
    }

    public void Start()
    {
        Logger.Debug<AppLog>($"Settings heartbeat scheduled in: {InitialDelay ?? TimeSpan.Zero}");

        StartTimer();
    }

    public void Stop()
    {
        StopTimer();
    }

    protected override async Task OnTriggerAsync()
    {
        try
        {
            await _settingsHeartbeatReporter.ReportAsync();

            _settings.LastSettingsHeartbeatTimeUtc = DateTimeOffset.UtcNow;
            Logger.Debug<AppLog>($"Settings heartbeat sent at {_settings.LastSettingsHeartbeatTimeUtc}");
        }
        catch (Exception ex)
        {
            Logger.Warn<AppLog>("Failed to send settings heartbeat", ex);
        }
    }

    private TimeSpan CalculateInitialDelay()
    {
        TimeSpan delay = TimeSpan.Zero;

        if (_settings.LastSettingsHeartbeatTimeUtc != null)
        {
            DateTimeOffset nextHeartbeatTime = _settings.LastSettingsHeartbeatTimeUtc.Value.Add(PollingInterval);
            delay = nextHeartbeatTime - DateTimeOffset.UtcNow;
        }

        return delay.Clamp(HEARTBEAT_MINIMUM_DELAY, HEARTBEAT_INTERVAL);
    }
}