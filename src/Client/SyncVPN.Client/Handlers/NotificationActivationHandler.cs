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

using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Handlers.Bases;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Notifications.Contracts;
using SyncVPN.Client.Notifications.Contracts.Arguments;
using SyncVPN.Client.Services.PortForwarding;
using SyncVPN.Client.Services.Upselling;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.Handlers;

public class NotificationActivationHandler : IHandler,
    IEventMessageReceiver<NotificationActivationMessage>
{
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IPortForwardingClipboardService _portForwardingClipboardService;
    private readonly IAccountUpgradeUrlLauncher _accountUpgradeUrlLauncher;

    public NotificationActivationHandler(
        IUIThreadDispatcher uiThreadDispatcher,
        IMainWindowActivator mainWindowActivator,
        IPortForwardingManager portForwardingManager,
        IPortForwardingClipboardService portForwardingClipboardService,
        IAccountUpgradeUrlLauncher accountUpgradeUrlLauncher)
    {
        _uiThreadDispatcher = uiThreadDispatcher;
        _mainWindowActivator = mainWindowActivator;
        _portForwardingClipboardService = portForwardingClipboardService;
        _accountUpgradeUrlLauncher = accountUpgradeUrlLauncher;
    }

    public void Receive(NotificationActivationMessage message)
    {
        HandleCustomActivationActionAsync(message.Argument);
    }

    private async void HandleCustomActivationActionAsync(string argument)
    {
        switch (argument)
        {
            case NotificationArguments.UPGRADE:
                await _accountUpgradeUrlLauncher.OpenAsync(ModalSource.Downgrade);
                break;
            case NotificationArguments.COPY_PORT_FORWARDING_PORT_TO_CLIPBOARD:
                _uiThreadDispatcher.TryEnqueue(async () =>
                {
                    await _portForwardingClipboardService.CopyActivePortToClipboardAsync();
                });
                break;
            default:
                _uiThreadDispatcher.TryEnqueue(_mainWindowActivator.Activate);
                break;
        }
    }
}