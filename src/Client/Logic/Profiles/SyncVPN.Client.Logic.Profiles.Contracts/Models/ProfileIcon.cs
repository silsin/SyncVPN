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

using SyncVPN.Client.Common.Enums;

namespace SyncVPN.Client.Logic.Profiles.Contracts.Models;

public class ProfileIcon : IProfileIcon
{
    public const ProfileColor DEFAULT_COLOR = ProfileColor.Purple;
    public const ProfileCategory DEFAULT_CATEGORY = ProfileCategory.Speed;

    public static IProfileIcon Default => new ProfileIcon() { Category = DEFAULT_CATEGORY, Color = DEFAULT_COLOR };

    public ProfileCategory Category { get; set; }

    public ProfileColor Color { get; set; }

    public IProfileIcon Copy()
    {
        return (IProfileIcon)MemberwiseClone();
    }
}