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

using SyncVPN.Logging.Contracts;
using SyncVPN.OperatingSystems.Registries.Contracts;

namespace SyncVPN.ProcessCommunication.Common.Registration;

public abstract class ServerPortRegisterBase
{
    private const string PATH = "SOFTWARE\\Proton AG\\SyncVPN\\gRPC";

    private readonly IRegistryEditor _registryEditor;
    private readonly RegistryUri _registryUri;

    protected ILogger Logger { get; private set; }

    protected ServerPortRegisterBase(IRegistryEditor registryEditor, ILogger logger)
    {
        _registryEditor = registryEditor;
        _registryUri = RegistryUri.CreateLocalMachineUri(PATH, GetKey());
        Logger = logger;
    }

    protected abstract string GetKey();

    public void Write(int serverBoundPort)
    {
        _registryEditor.WriteInt(_registryUri, serverBoundPort);
    }

    public void Delete()
    {
        _registryEditor.Delete(_registryUri);
    }

    public int? ReadOnce()
    {
        return _registryEditor.ReadInt(_registryUri);
    }
}
