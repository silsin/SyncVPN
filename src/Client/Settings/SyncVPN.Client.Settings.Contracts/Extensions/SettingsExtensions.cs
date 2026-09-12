/*
 * Copyright (c) 2024 Proton AG
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

using SyncVPN.Client.Settings.Contracts.Enums;

namespace SyncVPN.Client.Settings.Contracts.Extensions;

public static class SettingsExtensions
{
    public static bool IsAdvancedKillSwitchActive(this ISettings settings)
    {
        return settings.IsKillSwitchEnabled && settings.KillSwitchMode == KillSwitchMode.Advanced;
    }

    public static string GetUsername(this ISettings settings)
    {
        // Legacy (SRP/SSO) sessions populate Username/UserDisplayName; a SyncVPN-backend session (see
        // SyncVpnAuthenticator.StoreSession) never touches those and only sets SyncVpnUserName instead -
        // without this fallback, the Settings account button would show a blank name for those users.
        return settings.Username ?? settings.UserDisplayName ?? settings.SyncVpnUserName ?? string.Empty;
    }
}