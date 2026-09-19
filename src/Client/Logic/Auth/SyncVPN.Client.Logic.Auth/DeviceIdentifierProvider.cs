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

using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Crypto.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;
using SyncVPN.OperatingSystems.Registries.Contracts;

namespace SyncVPN.Client.Logic.Auth;

// Derives the device id from HKLM\SOFTWARE\Microsoft\Cryptography\MachineGuid, a value Windows itself
// generates once per installation - it survives this app being uninstalled and reinstalled (unlike a
// random GUID stored in app settings, which a clean reinstall regenerates), and differs across
// machines. Hashed rather than sent raw, so the backend gets a stable per-device id without this app
// handing out the real, cross-app-correlatable Windows machine identifier.
public class DeviceIdentifierProvider : IDeviceIdentifierProvider
{
    private static readonly RegistryUri MachineGuidUri =
        RegistryUri.CreateLocalMachineUri(@"SOFTWARE\Microsoft\Cryptography", "MachineGuid");

    private readonly IRegistryEditor _registryEditor;
    private readonly ISha1Calculator _sha1Calculator;
    private readonly ILogger _logger;

    public DeviceIdentifierProvider(IRegistryEditor registryEditor, ISha1Calculator sha1Calculator, ILogger logger)
    {
        _registryEditor = registryEditor;
        _sha1Calculator = sha1Calculator;
        _logger = logger;
    }

    public string GetStableDeviceId()
    {
        string? machineGuid = _registryEditor.ReadString(MachineGuidUri);
        if (string.IsNullOrWhiteSpace(machineGuid))
        {
            // Readable by any user on every Windows installation this app supports - only expected to
            // be missing/unreadable in a broken environment. Random fallback keeps registration working
            // rather than blocking it, at the cost of this one run not matching a future stable id.
            _logger.Error<AppLog>("Could not read MachineGuid from the registry - falling back to a random device id for this run.");
            return Guid.NewGuid().ToString();
        }

        return _sha1Calculator.Hash($"SyncVPN-Device-{machineGuid}");
    }
}
