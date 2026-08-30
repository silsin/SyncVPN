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

using SyncVPN.Api.Contracts.HumanVerification;
using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Services.Verification.Messages;

namespace SyncVPN.Client.Services.Verification;

public class HumanVerifier : IHumanVerifier,
    IEventMessageReceiver<ResponseTokenMessage>
{
    private readonly IMainWindowOverlayActivator _overlayActivator;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    private string _resolvedToken = string.Empty;

    public HumanVerifier(
        IMainWindowOverlayActivator overlayActivator,
        IEventMessageSender eventMessageSender,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _overlayActivator = overlayActivator;
        _eventMessageSender = eventMessageSender;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    public async Task<string> VerifyAsync(string token)
    {
        _eventMessageSender.Send(new RequestTokenMessage(token));

        await _uiThreadDispatcher.TryEnqueueAsync(_overlayActivator.ShowHumanVerificationOverlayAsync);

        return _resolvedToken;
    }

    public void Receive(ResponseTokenMessage message)
    {
        _resolvedToken = message.Token;
    }
}