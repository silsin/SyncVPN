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

using SyncVPN.Client.Common.Messages;
using SyncVPN.Client.Contracts.Services.Validation;
using SyncVPN.Client.Core.Messages;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Handlers.Bases;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Servers.Cache;

namespace SyncVPN.Client.Handlers;

public class ClientStartHandler : IHandler,
    IEventMessageReceiver<ApplicationStartingMessage>,
    IEventMessageReceiver<ApplicationStartedMessage>
{
    private readonly IServersCache _serversCache;
    private readonly IVpnStatePollingObserver _vpnStatePollingObserver;
    private readonly ISystemTimeValidator _systemTimeValidator;

    public ClientStartHandler(
        IServersCache serversCache,
        IVpnStatePollingObserver vpnStatePollingObserver,
        ISystemTimeValidator systemTimeValidator)
    {
        _serversCache = serversCache;
        _vpnStatePollingObserver = vpnStatePollingObserver;
        _systemTimeValidator = systemTimeValidator;
    }

    public void Receive(ApplicationStartingMessage message)
    {
        // If the client starts after crash while connected to VPN, we need to have 
        // servers cache loaded so ConnectionManager can map currently connected server
        // to the one in the servers cache.
        _serversCache.LoadFromFileIfEmpty();

        _vpnStatePollingObserver.Initialize();
    }

    public async void Receive(ApplicationStartedMessage message)
    {
        await _systemTimeValidator.CheckAsync(new CancellationTokenSource().Token);
    }
}