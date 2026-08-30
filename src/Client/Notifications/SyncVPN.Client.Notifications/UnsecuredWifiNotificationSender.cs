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
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.UnsecureWifiDetection.Contracts;
using SyncVPN.Logging.Contracts;

namespace SyncVPN.Client.Notifications;

public class UnsecuredWifiNotificationSender : NotificationSenderBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly ILocalizationProvider _localizer;

    private ConnectionStatus _connectionStatus;

    private string _currentUnsecureWifiName = string.Empty;

    private bool IsCurrentWifiSecure => string.IsNullOrEmpty(_currentUnsecureWifiName);

    public UnsecuredWifiNotificationSender(
        ILogger logger,
        ILocalizationProvider localizationProvider,
        INetworkClient networkClient)
        : base(logger)
    {
        _localizer = localizationProvider;

        networkClient.WifiChangeDetected += OnWifiChangeDetected;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (_connectionStatus == message.ConnectionStatus)
        {
            return;
        }

        _connectionStatus = message.ConnectionStatus;

        HandleNotification();
    }

    private void HandleNotification()
    {
        if (IsCurrentWifiSecure || _connectionStatus != ConnectionStatus.Disconnected)
        {
            return;
        }

        Send();
    }

    public void Send()
    {
        Send(new ToastContentBuilder()
            .AddText(_localizer.Get("Notifications_UnsecureWifi_Title"))
            .AddText(_localizer.Get("Notifications_UnsecureWifi_Description")));
    }

    private void OnWifiChangeDetected(object? sender, WifiChangeEventArgs e)
    {
        if (e.IsSecure)
        {
            _currentUnsecureWifiName = string.Empty;
        }
        else
        {
            _currentUnsecureWifiName = e.Name;

            HandleNotification();
        }
    }
}