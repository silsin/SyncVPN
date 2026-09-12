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

using System;

namespace SyncVPN.OperatingSystems.Ras.Contracts;

// One Windows RAS dial-up VPN connection (L2TP/IPsec-PSK or SSTP), backed by rasapi32.dll. An
// instance is single-use: Connect once, Disconnect (or Dispose) to tear down, then discard - create
// a new instance for the next attempt rather than reusing one.
public interface IRasConnection : IDisposable
{
    event EventHandler<RasStateChangedEventArgs> StateChanged;

    // Creates (or overwrites) a service-owned phonebook entry for the given options and starts
    // dialing it asynchronously. StateChanged reports progress; this method returns once the dial
    // attempt has been handed to RAS, not once it completes.
    void Connect(RasEntryOptions options);

    void Disconnect();

    // The local tunnel IP address negotiated over PPP, once Connected - null before then or if RAS
    // couldn't report one.
    string? GetLocalIpAddress();
}
