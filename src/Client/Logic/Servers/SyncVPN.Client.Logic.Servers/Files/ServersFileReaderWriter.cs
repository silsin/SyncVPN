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
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Configurations.Contracts;
using SyncVPN.Files.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;
using SyncVPN.Serialization.Contracts;

namespace SyncVPN.Client.Logic.Servers.Files;

public class ServersFileReaderWriter : IServersFileReaderWriter
{
    private const string FILE_NAME_PREFIX = "Servers";
    private const string FILE_EXTENSION = "bin";

    private readonly ILogger _logger;
    private readonly IStaticConfiguration _staticConfiguration;
    private readonly IUserFileReaderWriter _userFileReaderWriter;

    private readonly UserFileReaderWriterParameters _fileReaderWriterParameters;

    public ServersFileReaderWriter(ILogger logger,
        IStaticConfiguration staticConfiguration,
        IUserFileReaderWriter userFileReaderWriter)
    {
        _logger = logger;
        _staticConfiguration = staticConfiguration;
        _userFileReaderWriter = userFileReaderWriter;

        _fileReaderWriterParameters = new(Serializers.Protobuf, _staticConfiguration.StorageFolder, FILE_NAME_PREFIX, FILE_EXTENSION);
    }

    public ServersFile Read()
    {
        _logger.Info<AppLog>("Reading servers file.");
        ServersFile file = _userFileReaderWriter.ReadOrNew<ServersFile>(_fileReaderWriterParameters);
        _logger.Info<AppLog>($"Read {file.Servers.Count} servers from file.");
        return file;
    }

    public void Save(ServersFile file)
    {
        _logger.Info<AppLog>("Writing servers file.");
        FileOperationResult result = _userFileReaderWriter.Write(file, _fileReaderWriterParameters);
        _logger.Info<AppLog>($"Writing servers file finished with state '{result}'.");
    }
}