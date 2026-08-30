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
using SyncVPN.StatisticalEvents.Contracts.Models;
using SyncVPN.StatisticalEvents.MeasurementGroups;
using SyncVPN.StatisticalEvents.Dimensions.Builders;
using SyncVPN.StatisticalEvents.Events.Senders.Contracts;

namespace SyncVPN.StatisticalEvents;

public class VpnConnectionReporter : ReporterBase<VpnConnectionsMeasurementGroup>, IVpnConnectionReporter
{
    private readonly IVpnConnectionDimensionsBuilder _dimensionsBuilder;

    public override string Event => "vpn_connection";

    public VpnConnectionReporter(
        IVpnConnectionDimensionsBuilder dimensionsBuilder,
        IAuthenticatedStatisticalEventSender statisticalEventSender)
        : base(statisticalEventSender)
    {
        _dimensionsBuilder = dimensionsBuilder;
    }

    public void Report(VpnConnectionEventData eventData, float timeToConnectionInMilliseconds)
    {
        ReportEvent(
            CreateStatisticalEventBuilder()
                .WithDimensions(_dimensionsBuilder.Build(eventData))
                .WithDimensions(_dimensionsBuilder.BuildConnectionDimensions(eventData))
                .WithValue("time_to_connection", timeToConnectionInMilliseconds)
                .Build());
    }
}