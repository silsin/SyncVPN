/*
 * Copyright (c) 2025 Proton AG
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

using SyncVPN.UI.Tests.Robots;

namespace SyncVPN.UI.Tests.Extensions;

public static class ConfirmationRobotExtensions
{
    private const string EXCLUDED_LOCATIONS_DISCOVERY_PROMPT = "Not the country you wanted?";
    private const string EXCLUDED_LOCATIONS_DISCOVERY_ACTION = "Exclude locations";
    private const string EXCLUDED_LOCATIONS_DISCOVERY_CANCEL = "Skip";

    public static void DismissExcludedLocationsPrompt(this ConfirmationRobot robot)
    {
        robot.Verify.IsOverlayDisplayed()
                    .OverlayTextContains(EXCLUDED_LOCATIONS_DISCOVERY_PROMPT)
                    .OverlayButtonsEquals(
                         primary: EXCLUDED_LOCATIONS_DISCOVERY_ACTION,
                         cancel: EXCLUDED_LOCATIONS_DISCOVERY_CANCEL)
             .CancelAction();
    }
}
