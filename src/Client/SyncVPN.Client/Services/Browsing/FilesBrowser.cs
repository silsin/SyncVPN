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

using System.Diagnostics;
using SyncVPN.Client.Common.UI.Extensions;
using SyncVPN.Client.Contracts.Services.Browsing;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;

namespace SyncVPN.Client.Services.Browsing;

public class FilesBrowser : IFilesBrowser
{
    private readonly ILogger _logger;

    public FilesBrowser(ILogger logger)
    {
        _logger = logger;
    }

    public void OpenApp(string appPath)
    {
        if (!appPath.IsValidPath())
        {
            _logger.Warn<AppFileAccessFailedLog>($"Could not open the application. App could not be found: {appPath}");
            return;
        }

        string appName = appPath.GetAppName();

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = appPath,
                UseShellExecute = true
            });
        }
        catch (Exception e)
        {
            _logger.Error<AppFileAccessFailedLog>($"Could not open the application: {appName}", e);
        }
    }

    public void OpenFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            _logger.Warn<AppFileAccessFailedLog>($"Could not open the folder. Folder could not be found: {folderPath}");
            return;
        }

        string folderName = folderPath.GetFolderName();

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }
        catch (Exception e)
        {
            _logger.Error<AppFileAccessFailedLog>($"Could not open the folder: {folderName}", e);
        }
    }
}