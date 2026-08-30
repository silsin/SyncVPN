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

using SyncVPN.OperatingSystems.Network.Contracts;
using SyncVPN.OperatingSystems.Registries.Contracts;

namespace SyncVPN.OperatingSystems.Network;

public class ProxyDetector : IProxyDetector
{
    private const string REGISTRY_PATH = "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings";
    private const string REGISTRY_KEY = "ProxyEnable";

    private readonly RegistryUri _registryUri = RegistryUri.CreateCurrentUserUri(REGISTRY_PATH, REGISTRY_KEY);

    private readonly IRegistryEditor _registryEditor;

    public ProxyDetector(IRegistryEditor registryEditor)
    {
        _registryEditor = registryEditor;
    }

    public bool IsEnabled()
    {
        return _registryEditor.ReadInt(_registryUri) == 1;
    }
}