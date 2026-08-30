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

using SyncVPN.StatisticalEvents.Contracts;
using SyncVPN.StatisticalEvents.Dimensions.Builders;
using SyncVPN.StatisticalEvents.Events.Senders.Contracts;
using SyncVPN.StatisticalEvents.MeasurementGroups;

namespace SyncVPN.StatisticalEvents;

public class ProductPromptActionReporter : ReporterBase<ProductPromptsMeasurementGroup>, IProductPromptActionReporter
{
    private readonly IProductPromptDimensionsBuilder _dimensionsBuilder;

    public override string Event => "product_prompt_action";

    public ProductPromptActionReporter(
        IProductPromptDimensionsBuilder dimensionsBuilder,
        IAuthenticatedStatisticalEventSender statisticalEventSender)
        : base(statisticalEventSender)
    {
        _dimensionsBuilder = dimensionsBuilder;
    }

    public void Report(PromptType promptType, PromptContext promptContext, PromptAction promptAction)
    {
        ReportEvent(
            CreateStatisticalEventBuilder()
                .WithDimensions(_dimensionsBuilder.Build(promptType, promptContext))
                .WithDimensions(_dimensionsBuilder.BuildAction(promptAction))
                .Build());
    }
}