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

using SyncVPN.Client.Common.Models;
using SyncVPN.Client.Localization.Contracts;

namespace SyncVPN.Client.Settings.Contracts.Conflicts.Bases;

public abstract class SettingsConflictBase : ISettingsConflict
{
    protected readonly ILocalizationProvider Localizer;

    protected readonly ISettings Settings;

    public abstract string SettingsName { get; }

    public abstract dynamic? SettingsValue { get; }

    public abstract dynamic? SettingsResetValue { get; }

    public abstract bool IsConflicting { get; }

    public abstract MessageDialogParameters MessageParameters { get; }

    public abstract Action ResolveAction { get; }

    public abstract bool IsReconnectionRequired { get; }

    protected SettingsConflictBase(ILocalizationProvider localizer, ISettings settings)
    {
        Localizer = localizer;
        Settings = settings;
    }

    public bool Matches(string settingsName, dynamic? settingsValue)
    {
        return SettingsName == settingsName
            && SettingsValue == settingsValue;
    }
}