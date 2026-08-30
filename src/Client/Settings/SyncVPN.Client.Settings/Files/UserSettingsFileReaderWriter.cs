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

using SyncVPN.Client.Files.Contracts;
using SyncVPN.Configurations.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;
using SyncVPN.Serialization.Contracts;

namespace SyncVPN.Client.Settings.Files;

public class UserSettingsFileReaderWriter : IUserSettingsFileReaderWriter
{
    private const string FILE_NAME_PREFIX = "UserSettings";
    private const string FILE_EXTENSION = "json";

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IUserFileReaderWriter _userFileReaderWriter;

    private readonly UserFileReaderWriterParameters _fileReaderWriterParameters;

    public UserSettingsFileReaderWriter(ILogger logger,
        IStaticConfiguration staticConfiguration,
        IUserFileReaderWriter userFileReaderWriter)
    {
        _logger = logger;
        _staticConfiguration = staticConfiguration;
        _userFileReaderWriter = userFileReaderWriter;

        _fileReaderWriterParameters = new(Serializers.PrettyJson, _staticConfiguration.StorageFolder, FILE_NAME_PREFIX, FILE_EXTENSION);
    }

    public IDictionary<string, string?> Read()
    {
        _logger.Info<AppLog>($"Reading user settings file.");

        Dictionary<string, string?> settings = _userFileReaderWriter
            .ReadOrNew<Dictionary<string, string?>>(_fileReaderWriterParameters);

        _logger.Info<AppLog>($"Read {settings.Count} user settings from file.");
        return settings;
    }

    public void Write(IDictionary<string, string?> dictionary)
    {
        _userFileReaderWriter.Write(dictionary, _fileReaderWriterParameters);
    }
}