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
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.FreeServers;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts.Models;
using SyncVPN.IssueReporting.Contracts;
using SyncVPN.Logging.Contracts;

namespace SyncVPN.Client.Logic.Connection;

public class ChangeServerModerator :
    PollingObserverBase,
    IChangeServerModerator,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;
    private readonly IEventMessageSender _eventMessageSender;

    private ChangeServerSettings _changeServerSettings;
    private ChangeServerAttempts _changeServerAttempts;

    protected override TimeSpan PollingInterval { get; } = TimeSpan.FromSeconds(1);

    public ChangeServerModerator(ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings,
        IConnectionManager connectionManager,
        IEventMessageSender eventMessageSender)
        : base(logger, issueReporter)
    {
        _settings = settings;
        _connectionManager = connectionManager;
        _eventMessageSender = eventMessageSender;

        InvalidateChangeServerSettings();
        InvalidateChangeServerAttempts();
    }

    public bool CanChangeServer()
    {
        return GetRemainingDelayUntilNextAttempt() == TimeSpan.Zero;
    }

    public bool IsAttemptsLimitReached()
    {
        return _changeServerAttempts.AttemptsCount >= _changeServerSettings.AttemptsLimit;
    }

    public TimeSpan GetDelayUntilNextAttempt()
    {
        return IsAttemptsLimitReached()
            ? _changeServerSettings.LongDelay
            : _changeServerSettings.ShortDelay;
    }

    public TimeSpan GetRemainingDelayUntilNextAttempt()
    {
        TimeSpan delay = GetDelayUntilNextAttempt();

        TimeSpan expiredTime = DateTimeOffset.UtcNow - _changeServerAttempts.LastAttemptUtcDate;
        TimeSpan remainingTime = delay - expiredTime;

        return remainingTime <= TimeSpan.Zero
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds(Math.Ceiling(remainingTime.TotalSeconds));
    }

    public void Receive(LoggedInMessage message)
    {
        InvalidateChangeServerAttempts();
    }

    public void Receive(SettingChangedMessage message)
    {
        switch (message.PropertyName)
        {
            case nameof(ISettings.ChangeServerSettings):
                InvalidateChangeServerSettings();
                break;

            case nameof(ISettings.ChangeServerAttempts):
                InvalidateChangeServerAttempts();
                break;

            default:
                break;
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (_connectionManager.IsConnected &&
            _connectionManager.CurrentConnectionIntent?.Location is FreeServerLocationIntent intent &&
           intent.Strategy == SelectionStrategy.Random)
        {
            RegisterChangeServerAttempt();
        }

        InvalidateTimer();
    }

    protected override Task OnTriggerAsync()
    {
        InvalidateTimer();

        return Task.CompletedTask;
    }

    private void InvalidateTimer()
    {
        _eventMessageSender.Send(new ChangeServerAttemptInvalidatedMessage());

        if (_connectionManager.IsConnected && !CanChangeServer())
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }

    private void InvalidateChangeServerSettings()
    {
        _changeServerSettings = _settings.ChangeServerSettings;
    }

    private void InvalidateChangeServerAttempts()
    {
        _changeServerAttempts = _settings.ChangeServerAttempts;

        InvalidateTimer();
    }

    private void RegisterChangeServerAttempt()
    {
        _settings.ChangeServerAttempts = new()
        {
            AttemptsCount = IsAttemptsLimitReached() ? 1 : Math.Max(1, _changeServerAttempts.AttemptsCount + 1),
            LastAttemptUtcDate = DateTimeOffset.UtcNow,
        };
    }
}