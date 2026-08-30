/*
 * Copyright (c) 2024 Proton AG
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

using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Handlers.Bases;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.IssueReporting.Static;

namespace SyncVPN.Client.Handlers;

public class CrashReportSettingHandler : IHandler, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;

    public CrashReportSettingHandler(ISettings settings)
    {
        _settings = settings;
        Set();
    }

    private void Set()
    {
        IssueReportingInitializer.SetEnabled(_settings.IsShareCrashReportsEnabled);
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsShareCrashReportsEnabled))
        {
            Set();
        }
    }
}