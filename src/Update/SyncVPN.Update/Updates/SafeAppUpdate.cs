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

using System;
using System.Threading.Tasks;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppUpdateLogs;
using SyncVPN.Update.Contracts;

namespace SyncVPN.Update.Updates
{
    /// <summary>
    /// Suppresses and logs expected exceptions of <see cref="NotifyingAppUpdate.StartUpdating"/> method.
    /// </summary>
    public class SafeAppUpdate : INotifyingAppUpdate
    {
        private readonly INotifyingAppUpdate _origin;
        private readonly ILogger _logger;

        public SafeAppUpdate(ILogger logger, INotifyingAppUpdate origin)
        {
            _logger = logger;
            _origin = origin;
        }

        public event EventHandler<AppUpdateStateContract> StateChanged
        {
            add => _origin.StateChanged += value;
            remove => _origin.StateChanged -= value;
        }

        public void StartCheckingForUpdate(bool earlyAccess) => _origin.StartCheckingForUpdate(earlyAccess);

        public async Task StartUpdating(bool auto)
        {
            try
            {
                await _origin.StartUpdating(auto);
            }
            catch (AppUpdateException e)
            {
                _logger.Error<AppUpdateLog>("Error when starting update.", e);
            }
        }
    }
}