/*
 * Copyright (c) 2026 Proton AG
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

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts;
using SyncVPN.Api.V2.Contracts.Account;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;

namespace SyncVPN.Client.Services.ServerPing;

// The public server catalog (GET /servers) carries no IP for any server - see ServerListItem's
// remarks - so measuring a real round-trip time means claiming the server first, the same POST
// /account call used to actually connect. Claiming (and therefore measuring) each server at most once
// per process lifetime, cached by ServerId, keeps repeated list rebuilds from hammering that endpoint
// the way an unthrottled per-render ping would.
public class ServerPingService : IServerPingService
{
    private const int PingTimeoutMs = 2000;

    // WireGuard/OpenVpn-UDP servers don't answer on TCP - 443 is used only as a generic "is this host
    // up at all" fallback when ICMP is filtered, not a claim that the server listens there.
    private const int FallbackTcpPort = 443;

    private readonly ISyncVpnApiClient _syncVpnApiClient;
    private readonly ILogger _logger;

    private readonly ConcurrentDictionary<long, Task<int?>> _pings = new();

    public ServerPingService(ISyncVpnApiClient syncVpnApiClient, ILogger logger)
    {
        _syncVpnApiClient = syncVpnApiClient;
        _logger = logger;
    }

    public Task<int?> GetPingMsAsync(long serverId, string protocol, string? transport, CancellationToken cancellationToken = default)
    {
        return _pings.GetOrAdd(serverId, _ => MeasureAsync(serverId, protocol, transport, cancellationToken));
    }

    private async Task<int?> MeasureAsync(long serverId, string protocol, string? transport, CancellationToken cancellationToken)
    {
        try
        {
            ApiResponseResult<ClaimAccountResponse> response = await _syncVpnApiClient.ClaimAccountAsync(
                new ClaimAccountRequest { ServerId = serverId, Protocol = protocol, Transport = transport }, cancellationToken);

            string? ip = response.Value?.Data.Account.ServerIp;
            if (response.Failure || string.IsNullOrEmpty(ip))
            {
                _logger.Warn<AppLog>($"Ping: could not claim ServerId={serverId} to measure ping: {response.Error}");
                return null;
            }

            int? pingMs = await PingIcmpAsync(ip, cancellationToken) ?? await PingTcpAsync(ip, FallbackTcpPort, cancellationToken);
            _logger.Info<AppLog>($"Ping: ServerId={serverId} Ip={ip} measured " +
                (pingMs.HasValue ? $"{pingMs}ms." : "unreachable (ICMP and TCP fallback both failed)."));
            return pingMs;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.Warn<AppLog>($"Ping: measuring ServerId={serverId} failed.", ex);
            return null;
        }
    }

    private static async Task<int?> PingIcmpAsync(string ip, CancellationToken cancellationToken)
    {
        try
        {
            using Ping ping = new();
            PingReply reply = await ping.SendPingAsync(ip, PingTimeoutMs).WaitAsync(cancellationToken);
            return reply.Status == IPStatus.Success ? (int)reply.RoundtripTime : null;
        }
        catch (Exception ex) when (ex is PingException or OperationCanceledException or TimeoutException)
        {
            return null;
        }
    }

    private static async Task<int?> PingTcpAsync(string ip, int port, CancellationToken cancellationToken)
    {
        try
        {
            using TcpClient client = new();
            using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(PingTimeoutMs);

            Stopwatch stopwatch = Stopwatch.StartNew();
            await client.ConnectAsync(ip, port, timeoutSource.Token);
            stopwatch.Stop();

            return (int)stopwatch.ElapsedMilliseconds;
        }
        catch (Exception ex) when (ex is SocketException or OperationCanceledException)
        {
            return null;
        }
    }
}
