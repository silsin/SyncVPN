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

using SyncVPN.Client.Contracts.Services.Activation;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;

namespace SyncVPN.Client.Logic.Connection.ConnectionErrors;

public class UnknownConnectionError : ReportableConnectionError
{
    private VpnError _lastError;

    public override string Message => Localizer.GetFormat("Connection_Error_Unknown", (int)_lastError);

    public UnknownConnectionError(
        ILocalizationProvider localizer,
        IClientWindowsActivator clientWindowsActivator)
        : base(localizer, clientWindowsActivator)
    { }

    public void SetLastError(VpnError error)
    {
        _lastError = error;
    }
}