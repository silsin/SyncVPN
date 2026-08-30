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

using System.Collections.Generic;
using System.Threading.Tasks;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.Contracts.Common;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.Common.Core.StatisticalEvents;
using SyncVPN.Configurations.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.StatisticalEvents.Contracts.Models;
using SyncVPN.StatisticalEvents.Events.Senders.Contracts;
using SyncVPN.StatisticalEvents.Files;

namespace SyncVPN.StatisticalEvents.Events.Senders;

public class AuthenticatedStatisticalEventSender : StatisticEventSenderBase, IAuthenticatedStatisticalEventSender,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private bool _isLoggedIn;

    protected override bool IsShareStatisticsEnabled => Settings.IsShareStatisticsEnabled;
    protected override bool CanSendTelemetryEvents => _isLoggedIn && IsShareStatisticsEnabled;

    public AuthenticatedStatisticalEventSender(
        IApiClient api,
        ILogger logger,
        ISettings settings,
        IConfiguration config,
        IStatisticalEventsFileReaderWriter statisticalEventsFileReaderWriter)
        : base(api, logger, settings, config, statisticalEventsFileReaderWriter)
    {
    }

    protected async override Task<ApiResponseResult<BaseResponse>> SendApiRequestAsync(
        StatisticalEventsBatch statisticalEventsBatch)
    {
        return await Api.PostAuthenticatedStatisticalEventsAsync(statisticalEventsBatch);
    }

    protected override List<StatisticalEvent> GetStatisticalEventsFromFile()
    {
        return StatisticalEventsFileReaderWriter.ReadAuthenticatedEvents().StatisticalEvents ?? [];
    }

    protected override void SaveToFile(List<StatisticalEvent> events)
    {
        StatisticalEventsFileReaderWriter.SaveAuthenticatedEvents(new StatisticalEventsFile()
        {
            StatisticalEvents = events
        });
    }

    public async void Receive(LoggedInMessage message)
    {
        _isLoggedIn = true;
        await StartAsync();
    }

    public async void Receive(LoggedOutMessage message)
    {
        _isLoggedIn = false;
        await StopAsync();
    }

    public async void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsShareStatisticsEnabled) &&
            !IsShareStatisticsEnabled)
        {
            await Semaphore.WaitAsync();
            try
            {
                ClearEventsDueToDisabledTelemetry();
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}