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

using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Handlers.Bases;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Notifications.Contracts;
using SyncVPN.Client.Settings.Contracts;

namespace SyncVPN.Client.Handlers;

public class PortForwardingNotificationHandler : IHandler, IEventMessageReceiver<PortForwardingPortChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IPortForwardingNewPortNotificationSender _portForwardingNewPortNotificationSender;
    private readonly IPortForwardingManager _portForwardingManager;

    public PortForwardingNotificationHandler(
        ISettings settings,
        IPortForwardingNewPortNotificationSender portForwardingNewPortNotificationSender,
        IPortForwardingManager portForwardingManager)
    {
        _settings = settings;
        _portForwardingNewPortNotificationSender = portForwardingNewPortNotificationSender;
        _portForwardingManager = portForwardingManager;
    }

    public void Receive(PortForwardingPortChangedMessage message)
    {
        int? activePortNumber = _portForwardingManager.ActivePort;
        if (activePortNumber is not null && _settings.IsPortForwardingNotificationEnabled)
        {
            _portForwardingNewPortNotificationSender.Send(activePortNumber.Value);
        }
    }
}