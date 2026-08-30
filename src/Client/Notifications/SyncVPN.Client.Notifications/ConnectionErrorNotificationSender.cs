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
using SyncVPN.Client.Notifications.Contracts;
using SyncVPN.Logging.Contracts;

namespace SyncVPN.Client.Notifications;

public class ConnectionErrorNotificationSender : NotificationSenderBase, IConnectionErrorNotificationSender,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private readonly ILocalizationProvider _localizer;

    private bool _isMainWindowVisible;

    public ConnectionErrorNotificationSender(
        ILogger logger,
        ILocalizationProvider localizer)
        : base(logger)
    {
        _localizer = localizer;
    }

    public void SendSessionLimitNotification()
    {
        SendNotification(
            title: _localizer.Get("SystemNotification_Disconnected"),
            message: _localizer.Get("Notifications_SessionLimit_Description"));
    }

    public void SendTwoFactorRequiredNotification()
    {
        SendNotification(
            title: _localizer.Get("Connection_Error_TwoFactorRequired_Title"),
            message: _localizer.Get("Connection_Error_TwoFactorRequired_Description"));
    }

    private void SendNotification(string title, string message)
    {
        if (_isMainWindowVisible)
        {
            return;
        }

        ToastContentBuilder tcb = new ToastContentBuilder()
            .AddText(title, AdaptiveTextStyle.Header)
            .AddText(message);

        Send(tcb);
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        _isMainWindowVisible = message.IsMainWindowVisible;
    }
}