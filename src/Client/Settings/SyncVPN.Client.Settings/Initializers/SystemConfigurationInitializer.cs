/*
 * Copyright (c) 2025 Proton AG
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

using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Initializers;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;
using SyncVPN.OperatingSystems.Antiviruses.Contracts;

namespace SyncVPN.Client.Settings.Initializers;

public class SystemConfigurationInitializer : ISystemConfigurationInitializer
{
    private const int PROLONGED_WIREGUARD_TIMEOUT_SECONDS = 15;

    private readonly IAntivirusProductsProvider _antivirusProductsProvider;
    private readonly ISettings _settings;
    private readonly ILogger _logger;

    public SystemConfigurationInitializer(
        IAntivirusProductsProvider antivirusProductsProvider,
        ISettings settings,
        ILogger logger)
    {
        _antivirusProductsProvider = antivirusProductsProvider;
        _settings = settings;
        _logger = logger;
    }

    public void Initialize()
    {
        ConfigureWireGuardTimeoutBasedOnAntivirusProducts();
    }

    private void ConfigureWireGuardTimeoutBasedOnAntivirusProducts()
    {
        List<string> antivirusProducts = _antivirusProductsProvider.GetAntivirusProducts();
        if (antivirusProducts.Count > 0)
        {
            _logger.Info<AppLog>($"Detected antivirus products: {string.Join(", ", antivirusProducts)}." +
                $" Changing WireGuard timeout to {PROLONGED_WIREGUARD_TIMEOUT_SECONDS} seconds.");
            _settings.WireGuardConnectionTimeout = TimeSpan.FromSeconds(PROLONGED_WIREGUARD_TIMEOUT_SECONDS);
        }
        else
        {
            _settings.WireGuardConnectionTimeout = DefaultSettings.WireGuardConnectionTimeout;
        }
    }
}