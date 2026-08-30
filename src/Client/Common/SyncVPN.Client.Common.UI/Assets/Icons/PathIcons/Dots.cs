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

using SyncVPN.Client.Common.UI.Assets.Icons.Base;

namespace SyncVPN.Client.Common.UI.Assets.Icons.PathIcons;

public class Dots : CustomPathIcon
{
    protected override string IconGeometry16 { get; }
        = "M6 3a1 1 0 1 0 0 2 1 1 0 0 0 0-2Zm0 4a1 1 0 1 0 0 2 1 1 0 0 0 0-2Zm-1 5a1 1 0 1 1 2 0 1 1 0 0 1-2 0Zm5-9a1 1 0 1 0 0 2 1 1 0 0 0 0-2ZM9 8a1 1 0 1 1 2 0 1 1 0 0 1-2 0Zm1 3a1 1 0 1 0 0 2 1 1 0 0 0 0-2Z";

    protected override string IconGeometry20 { get; }
        = ""; 

    protected override string IconGeometry24 { get; }
        = ""; 

    protected override string IconGeometry32 { get; }
        = "";
}