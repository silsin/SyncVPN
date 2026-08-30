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

using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;
using SyncVPN.OperatingSystems.PowerEvents.Contracts;

namespace SyncVPN.OperatingSystems.PowerEvents;

public class PowerEventNotifier : IPowerEventNotifier
{
    private readonly ILogger _logger;

    public event EventHandler OnResume;

    public PowerEventNotifier(ILogger logger)
    {
        _logger = logger;
        try
        {
            SystemPowerNotifications.PowerModeChanged += OnPowerModeChanged;
        }
        catch (Exception ex)
        {
            logger.Error<AppLog>("Failed to register system power notifications.", ex);
        }
    }

    private void OnPowerModeChanged(object sender, PowerNotificationArgs args)
    {
        _logger.Debug<AppLog>($"Power mode changed to {args.Mode} (IsMonitorOn: {args.IsMonitorOn})");

        if (args.Mode == PowerBroadcastType.PBT_APMRESUMEAUTOMATIC)
        {
            OnResume?.Invoke(this, EventArgs.Empty);
        }
    }
}