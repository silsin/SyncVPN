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

using SyncVPN.StatisticalEvents.Contracts;
using SyncVPN.StatisticalEvents.Dimensions.Mappers.Bases;

namespace SyncVPN.StatisticalEvents.Dimensions.Mappers;

public class PromptActionDimensionMapper : DimensionMapperBase, IPromptActionDimensionMapper
{
    private const string CONFIGURE = "configure";
    private const string DISMISS = "dismiss";
    private const string LEARN_MORE = "learn_more";

    public string Map(PromptAction promptAction)
    {
        return promptAction switch
        {
            PromptAction.Configure => CONFIGURE,
            PromptAction.Dismiss => DISMISS,
            PromptAction.LearnMore => LEARN_MORE,
            _ => NOT_AVAILABLE
        };
    }
}
