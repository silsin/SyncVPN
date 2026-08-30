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

using System.Timers;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Logic.Services.Contracts;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Common.Core.Networking;
using SyncVPN.Common.Legacy.Abstract;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;
using Timer = System.Timers.Timer;

namespace SyncVPN.Client.Logic.Connection.NetworkingTraffic;

public class NetworkTrafficScheduler : INetworkTrafficScheduler,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly IEntityMapper _entityMapper;
    private readonly Timer _timer;

    public event EventHandler<NetworkTraffic>? NetworkTrafficChanged;

    public NetworkTrafficScheduler(IVpnServiceCaller vpnServiceCaller,
        IEntityMapper entityMapper)
    {
        _vpnServiceCaller = vpnServiceCaller;
        _entityMapper = entityMapper;

        _timer = new(TimeSpan.FromSeconds(1));
        _timer.Elapsed += OnTimerTickAsync;
        _timer.AutoReset = true;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        _timer.Enabled = message.ConnectionStatus is ConnectionStatus.Connected;
    }

    private async void OnTimerTickAsync(object? sender, ElapsedEventArgs e)
    {
        Result<NetworkTrafficIpcEntity> result = await _vpnServiceCaller.GetNetworkTrafficAsync();

        DateTime date = DateTime.UtcNow.TruncateToSeconds();

        NetworkTraffic networkTraffic = result.Success
            ? _entityMapper.Map<NetworkTrafficIpcEntity, NetworkTraffic>(result.Value)
            : NetworkTraffic.Zero;

        NetworkTrafficChanged?.Invoke(this, networkTraffic.Copy(date));
    }
}