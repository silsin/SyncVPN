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

using System.Collections.Generic;
using SyncVPN.Common.Core.StatisticalEvents;

namespace SyncVPN.StatisticalEvents.Events.Builders;

public class StatisticalEventBuilder
{
    private readonly StatisticalEvent _statisticalEvent;

    public StatisticalEventBuilder(StatisticalEvent statisticalEvent)
    {
        _statisticalEvent = statisticalEvent;
    }

    public StatisticalEventBuilder WithDimension(string key, string value)
    {
        _statisticalEvent.Dimensions[key] = value;
        return this;
    }

    public StatisticalEventBuilder WithDimensions(IReadOnlyDictionary<string, string> dimensions)
    {
        foreach ((string key, string value) in dimensions)
        {
            _statisticalEvent.Dimensions[key] = value;
        }

        return this;
    }

    public StatisticalEventBuilder WithValue(string key, float value)
    {
        _statisticalEvent.Values[key] = value;
        return this;
    }

    public StatisticalEventBuilder WithValues(IReadOnlyDictionary<string, float> values)
    {
        foreach ((string key, float value) in values)
        {
            _statisticalEvent.Values[key] = value;
        }

        return this;
    }

    public StatisticalEvent Build()
    {
        return _statisticalEvent;
    }
}
