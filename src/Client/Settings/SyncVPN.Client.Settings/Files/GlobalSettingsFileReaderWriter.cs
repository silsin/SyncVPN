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

using SyncVPN.Configurations.Contracts;
using SyncVPN.Files.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;
using SyncVPN.Serialization.Contracts;

namespace SyncVPN.Client.Settings.Files;

public class GlobalSettingsFileReaderWriter : IGlobalSettingsFileReaderWriter
{
    private const string FILE_NAME = "GlobalSettings.json";
    private const Serializers SERIALIZER = Serializers.PrettyJson;

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IFileReaderWriter _fileReaderWriter;

    private readonly string _fullFilePath;

    public GlobalSettingsFileReaderWriter(ILogger logger,
        IStaticConfiguration staticConfiguration,
        IFileReaderWriter fileReaderWriter)
    {
        _logger = logger;
        _staticConfiguration = staticConfiguration;
        _fileReaderWriter = fileReaderWriter;

        _fullFilePath = Path.Combine(_staticConfiguration.StorageFolder, FILE_NAME);
    }

    public IDictionary<string, string?> Read()
    {
        _logger.Info<AppLog>($"Reading global settings file '{_fullFilePath}'.");

        Dictionary<string, string?> settings = _fileReaderWriter.ReadOrNew<Dictionary<string, string?>>(_fullFilePath, SERIALIZER);

        _logger.Info<AppLog>($"Read {settings.Count} global settings from file '{_fullFilePath}'.");
        return settings;
    }

    public void Write(IDictionary<string, string?> dictionary)
    {
        _fileReaderWriter.Write(dictionary, _fullFilePath, SERIALIZER);
    }
}