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

using System.Management;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.OperatingSystemLogs;
using SyncVPN.OperatingSystems.Antiviruses.Contracts;

namespace SyncVPN.OperatingSystems.Antiviruses;

public class AntivirusProductsProvider : IAntivirusProductsProvider
{
    private const string WINDOWS_DEFENDER_NAME = "Windows Defender";

    private readonly ILogger _logger;

    public AntivirusProductsProvider(ILogger logger)
    {
        _logger = logger;
    }

    public List<string> GetAntivirusProducts()
    {
        List<string> list = [];

        try
        {
            using (ManagementObjectSearcher searcher = new(@"\\.\root\SecurityCenter2", "SELECT * FROM AntiVirusProduct"))
            using (ManagementObjectCollection results = searcher.Get())
            {
                foreach (ManagementObject mo in results.Cast<ManagementObject>())
                {
                    string? displayName = Convert.ToString(mo["displayName"]);
                    if (!string.IsNullOrEmpty(displayName) && displayName != WINDOWS_DEFENDER_NAME)
                    {
                        list.Add(displayName);
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error<OperatingSystemLog>("Failed to get antivirus products", e);
        }

        return list;
    }
}