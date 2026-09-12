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

namespace SyncVPN.OperatingSystems.Ras.Contracts;

// Everything needed to create and dial a single Windows RAS VPN entry. There is deliberately no
// custom-port option here: the native RAS L2TP/IPsec and SSTP clients always use the protocols'
// standard ports (UDP 500/1701/4500 for L2TP/IPsec NAT-T, TCP 443 for SSTP) - RAS itself has no
// facility to redirect them elsewhere. A backend that hands out non-standard SSTP ports cannot be
// reached through this engine; see the caller for how that's handled.
public class RasEntryOptions
{
    public required string EntryName { get; init; }
    public required string Server { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }

    // IPsec pre-shared key - required for L2TP, must be null for SSTP.
    public string? PreSharedKey { get; init; }

    public required RasDeviceType DeviceType { get; init; }
}
