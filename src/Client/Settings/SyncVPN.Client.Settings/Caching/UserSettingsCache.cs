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

using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.Client.Settings.Files;
using SyncVPN.Client.Settings.Repositories.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Serialization.Contracts.Json;

namespace SyncVPN.Client.Settings.Repositories;

public class UserSettingsCache : SettingsCacheBase, IUserSettingsCache, IEventMessageReceiver<SettingChangedMessage>
{
    public UserSettingsCache(ILogger logger,
        IJsonSerializer jsonSerializer,
        IEventMessageSender eventMessageSender,
        IUserSettingsFileReaderWriter userSettingsFileReaderWriter)
        : base(logger, jsonSerializer, eventMessageSender, userSettingsFileReaderWriter)
    {
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.UserId))
        {
            JsonCache.Reset();
            Cache.Clear();
        }
    }
}