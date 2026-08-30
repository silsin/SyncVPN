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

using SyncVPN.Client.Common.Dispatching;

namespace SyncVPN.Client.Services.Dispatching;

public class DispatcherTimer : IDispatcherTimer
{
    private readonly Microsoft.UI.Xaml.DispatcherTimer _timer;

    public event EventHandler<object>? Tick
    {
        add => _timer.Tick += value;
        remove => _timer.Tick += value;
    }

    public TimeSpan Interval
    {
        get => _timer.Interval;
        set => _timer.Interval = value;
    }   

    public bool IsEnabled => _timer.IsEnabled;

    public DispatcherTimer(TimeSpan interval)
    {
        _timer = new Microsoft.UI.Xaml.DispatcherTimer
        {
            Interval = interval
        };
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }
}