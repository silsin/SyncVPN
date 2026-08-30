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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppUpdateLogs;
using SyncVPN.Update.Responses;

namespace SyncVPN.Update.Releases;

/// <summary>
/// Transforms deserialized release data (stream of <see cref="CategoryResponse"/>) into stream of <see cref="Release"/>.
/// </summary>
public class Releases : IEnumerable<Release>
{
    private readonly ILogger _logger;
    private readonly IEnumerable<ReleaseResponse> _releases;
    private readonly Version _currentVersion;
    private readonly string _earlyAccessCategoryName;

    public Releases(ILogger logger, IEnumerable<ReleaseResponse> releases, Version currentVersion, string earlyAccessCategoryName)
    {
        _logger = logger;
        _releases = releases;
        _currentVersion = currentVersion;
        _earlyAccessCategoryName = earlyAccessCategoryName;
    }

    public IEnumerator<Release> GetEnumerator()
    {
        foreach (ReleaseResponse release in _releases)
        {
            if (release == null)
            {
                continue;
            }

            bool isEarlyAccess = string.Equals(_earlyAccessCategoryName, release.CategoryName, StringComparison.OrdinalIgnoreCase);
            if (Version.TryParse(release.Version, out Version version))
            {
                yield return new Release
                {
                    ChangeLog = release.ReleaseNotes.FirstOrDefault()?.Notes ?? [],
                    IsEarlyAccess = isEarlyAccess,
                    File = release.File,
                    IsNew = version > _currentVersion,
                    Version = version,
                    ReleaseDate = release.ReleaseDate
                };
            }
            else
            {
                _logger.Error<AppUpdateLog>($"Failed to parse release version {release.Version}.");
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}