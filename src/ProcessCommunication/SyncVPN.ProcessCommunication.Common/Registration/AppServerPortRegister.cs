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
using SyncVPN.ProcessCommunication.Contracts.Registration;

namespace SyncVPN.ProcessCommunication.Common.Registration;

public class AppServerPortRegister : ServerPortRegisterBase, IAppServerPortRegister
{
    private const string KEY = "AppServerPort";

    public AppServerPortRegister(IRegistryEditor registryEditor, ILogger logger)
        : base(registryEditor, logger)
    {
    }

    protected override string GetKey()
    {
        return KEY;
    }
}