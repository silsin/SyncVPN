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

using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Crypto.Contracts;

namespace SyncVPN.Client.Logic.Auth;

public class UserHashGenerator : IUserHashGenerator
{
    private readonly IGlobalSettings _globalSettings;
    private readonly ISha1Calculator _sha1Calculator;

    public UserHashGenerator(
        IGlobalSettings globalSettings,
        ISha1Calculator sha1Calculator)
    {
        _globalSettings = globalSettings;
        _sha1Calculator = sha1Calculator;
    }

    public string? Generate()
    {
        // Falls back to the (also persistent) SyncVpnDeviceId when nobody is logged in, so
        // per-user settings (UserFileReaderWriter) still have somewhere to persist for the
        // anonymous free-tier flow instead of failing every write - see UserFileReaderWriter.
        string? id = _globalSettings.UserId ?? _globalSettings.SyncVpnDeviceId;
        return id is null ? null : _sha1Calculator.Hash(id);
    }
}