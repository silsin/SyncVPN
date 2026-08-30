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

using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.Client.Logic.Connection.Contracts.RequestCreators;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Settings;

namespace SyncVPN.Client.Logic.Connection.RequestCreators;

public abstract class RequestCreatorBase
{
    protected readonly ILogger Logger;
    protected readonly ISettings Settings;
    protected readonly IEntityMapper EntityMapper;

    private readonly IMainSettingsRequestCreator _mainSettingsRequestCreator;

    protected RequestCreatorBase(
        ILogger logger,
        ISettings settings,
        IEntityMapper entityMapper,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
    {
        Logger = logger;
        Settings = settings;
        EntityMapper = entityMapper;

        _mainSettingsRequestCreator = mainSettingsRequestCreator;
    }

    protected MainSettingsIpcEntity GetSettings(IConnectionIntent? connectionIntent = null)
    {
        return _mainSettingsRequestCreator.Create(connectionIntent);
    }
}