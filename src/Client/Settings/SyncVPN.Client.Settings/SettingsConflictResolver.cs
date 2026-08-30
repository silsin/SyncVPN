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
using SyncVPN.Client.Settings.Contracts.Conflicts.Bases;
using SyncVPN.Client.Settings.Contracts.Messages;

namespace SyncVPN.Client.Settings;

public class SettingsConflictResolver : ISettingsConflictResolver, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly List<ISettingsConflict> _conflicts;

    public SettingsConflictResolver(IEnumerable<ISettingsConflict> conflicts)
    {
        _conflicts = conflicts.ToList();
    }

    public ISettingsConflict? GetConflict(string settingsName, dynamic? settingsValue)
    {
        return _conflicts.FirstOrDefault(c => c.Matches(settingsName, settingsValue) && c.IsConflicting);
    }

    public void ResolveConflict(string settingsName, dynamic? settingsValue)
    {
        ISettingsConflict? conflict = GetConflict(settingsName, settingsValue);

        conflict?.ResolveAction();
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message == null)
        {
            return;
        }

        ResolveConflict(message.PropertyName, message.NewValue);
    }
}