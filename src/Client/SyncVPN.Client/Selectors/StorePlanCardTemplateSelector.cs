/*
 * Copyright (c) 2026 Proton AG
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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.UI.Main.Store;

namespace SyncVPN.Client.Selectors;

public class StorePlanCardTemplateSelector : DataTemplateSelector
{
    public DataTemplate? CurrentPlanTemplate { get; set; }

    public DataTemplate? RecommendedPlanTemplate { get; set; }

    public DataTemplate? DefaultPlanTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        DataTemplate? template = item switch
        {
            StorePlanItem { IsCurrentPlan: true } => CurrentPlanTemplate,
            StorePlanItem { IsRecommended: true } => RecommendedPlanTemplate,
            StorePlanItem => DefaultPlanTemplate,
            _ => throw new NotSupportedException($"Store plan item {item} is not recognized"),
        };

        return template ?? throw new NotSupportedException("Store plan card data template is undefined");
    }
}
