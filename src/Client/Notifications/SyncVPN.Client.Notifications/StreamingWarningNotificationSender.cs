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

using Microsoft.Toolkit.Uwp.Notifications;
using SyncVPN.Client.Contracts.Messages;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Notifications.Contracts.Arguments;
using SyncVPN.Logging.Contracts;

namespace SyncVPN.Client.Notifications;

public class StreamingWarningNotificationSender : NotificationSenderBase, IStreamingWarningNotificationSender,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private readonly ILocalizationProvider _localizer;

    private bool _isMainWindowVisible;

    public StreamingWarningNotificationSender(
        ILogger logger,
        ILocalizationProvider localizer)
        : base(logger)
    {
        _localizer = localizer;
    }

    public void Send()
    {
        if (_isMainWindowVisible)
        {
            return;
        }

        ToastContentBuilder tcb = new();
        tcb.AddText(_localizer.Get("SystemNotification_StreamingWarning_Title"));
        tcb.AddText(_localizer.Get("SystemNotification_StreamingWarning_Description"));
        tcb.AddButton(_localizer.Get("Common_Actions_Upgrade"), ToastActivationType.Foreground, NotificationArguments.UPGRADE);
        Send(tcb);
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        _isMainWindowVisible = message.IsMainWindowVisible;
    }
}