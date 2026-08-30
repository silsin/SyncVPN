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

using SyncVPN.Api.Contracts;
using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Common.Messages;
using SyncVPN.Client.Core.Messages;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Handlers.Bases;
using SyncVPN.Client.Logic.Auth.Contracts.Enums;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;

namespace SyncVPN.Client.Handlers;

public class OutdatedClientHandler : IHandler, IOutdatedClientNotifier,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<HomePageDisplayedAfterLoginMessage>,
    IEventMessageReceiver<ApplicationStartedMessage>
{
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    private object _lock = new();

    private bool _isClientOutdated;
    private bool _isClientNotified;

    public OutdatedClientHandler(
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IEventMessageSender eventMessageSender,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _eventMessageSender = eventMessageSender;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    public void Receive(LoggedOutMessage message)
    {
        if (message.Reason != LogoutReason.ClientOutdated)
        {
            return;
        }

        NotifyOutdatedClient();
    }

    public void OnClientOutdated()
    {
        _isClientOutdated = true;
    }

    public void Receive(HomePageDisplayedAfterLoginMessage message)
    {
        if (_isClientOutdated)
        {
            _eventMessageSender.Send<ClientOutdatedMessage>();
        }
    }

    public void Receive(ApplicationStartedMessage message)
    {
        NotifyOutdatedClient();
    }

    private void NotifyOutdatedClient()
    {
        if (!_isClientOutdated)
        {
            return;
        }

        lock (_lock)
        {
            if (_isClientNotified)
            {
                return;
            }

            _isClientNotified = true;

            _uiThreadDispatcher.TryEnqueue(async () =>
            {
                await _mainWindowOverlayActivator.ShowOutdatedClientOverlayAsync();
            });
        }
    }
}