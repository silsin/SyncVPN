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

using System;
using System.Collections.Generic;
using SyncVPN.Common.Core.Networking;
using SyncVPN.Common.Legacy;
using SyncVPN.Common.Legacy.Threading;
using SyncVPN.Common.Legacy.Vpn;
using SyncVPN.Vpn.Common;

namespace SyncVPN.Vpn.Connection
{
    /// <summary>
    /// Queues <see cref="Connect"/>, <see cref="Disconnect"/>, and <see cref="UpdateServers"/>
    /// requests into sequence with events.
    /// A wrapper around <see cref="ISingleVpnConnection"/>.
    /// </summary>
    /// <remarks>
    /// Other wrappers behind <see cref="QueuingRequestsWrapper"/> will receive <see cref="Connect"/>,
    /// <see cref="Disconnect"/>, and <see cref="UpdateServers"/> requests queued into single queue.
    /// The next request will arrive only after previous one has passed the wrapper sequence behind
    /// <see cref="QueuingRequestsWrapper"/>.
    ///
    /// Requests and events should be processed fast without delays.
    /// </remarks>
    public class QueuingRequestsWrapper : IVpnConnection
    {
        private readonly IVpnConnection _origin;
        private readonly ITaskQueue _taskQueue;

        public QueuingRequestsWrapper(
            ITaskQueue taskQueue,
            IVpnConnection origin)
        {
            _taskQueue = taskQueue;
            _origin = origin;
        }

        public event EventHandler<EventArgs<VpnState>> StateChanged
        {
            add => _origin.StateChanged += value;
            remove => _origin.StateChanged -= value;
        }

        public event EventHandler<ConnectionDetails> ConnectionDetailsChanged
        {
            add => _origin.ConnectionDetailsChanged += value;
            remove => _origin.ConnectionDetailsChanged -= value;
        }

        public NetworkTraffic NetworkTraffic => _origin.NetworkTraffic;

        public void Connect(IReadOnlyList<VpnHost> servers, VpnConfig config, VpnCredentials credentials)
        {
            _taskQueue.Enqueue(() => _origin.Connect(servers, config, credentials));
        }

        public void ResetConnection()
        {
            _origin.ResetConnection();
        }

        public void Disconnect(VpnError error = VpnError.None)
        {
            _origin.Disconnect(error);
        }

        public void SetFeatures(VpnFeatures vpnFeatures)
        {
            _origin.SetFeatures(vpnFeatures);
        }

        public void RequestNetShieldStats()
        {
            _taskQueue.Enqueue(_origin.RequestNetShieldStats);
        }

        public void RequestConnectionDetails()
        {
            _taskQueue.Enqueue(_origin.RequestConnectionDetails);
        }
    }
}