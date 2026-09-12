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

public class RasStateChangedEventArgs : EventArgs
{
    public RasStateChangedEventArgs(RasConnectionState state, uint errorCode = 0, string? errorMessage = null)
    {
        State = state;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public RasConnectionState State { get; }

    // Raw RAS error code (from the RASDIALFUNC2 callback or a failed RasDial/RasSetEntryProperties
    // call) - 0 when the transition wasn't error-driven. See RasGetErrorString for a human message.
    public uint ErrorCode { get; }

    public string? ErrorMessage { get; }
}
