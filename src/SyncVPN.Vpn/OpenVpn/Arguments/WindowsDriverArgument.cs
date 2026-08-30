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

using System.Collections;
using System.Collections.Generic;

namespace SyncVPN.Vpn.OpenVpn.Arguments;

public class WindowsDriverArgument : IEnumerable<string>
{
    private readonly string _driver;

    public WindowsDriverArgument(string driver)
    {
        _driver = driver;
    }

    public IEnumerator<string> GetEnumerator()
    {
        yield return $"--windows-driver {_driver}";
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}