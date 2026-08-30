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
using System;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using SyncVPN.Logging.Contracts.Events.AppLogs;

namespace SyncVPN.Vpn.SynchronizationEvent
{
    /// <summary>
    /// Provides access to system synchronization events.
    /// </summary>
    internal class SystemSynchronizationEvents : ISynchronizationEvents
    {
        private readonly ILogger _logger;

        public SystemSynchronizationEvents(ILogger logger)
        {
            _logger = logger;
        }

        public ISynchronizationEvent SynchronizationEvent(string eventName)
        {
            try
            {
                if (EventWaitHandle.TryOpenExisting(eventName, out EventWaitHandle eventWaitHandle))
                {
                    return new SystemSynchronizationEvent(eventWaitHandle);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                LogException(ex);
            }
            catch (IOException ex)
            {
                LogException(ex);
            }
            return new NullSynchronizationEvent(); 

            void LogException(Exception e)
            {
                _logger.Warn<AppLog>($"Synchronization: Failed to open event {eventName}.", e);
            }
        }
    }
}
