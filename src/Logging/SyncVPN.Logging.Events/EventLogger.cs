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

using System.Diagnostics;

namespace SyncVPN.Logging.Events;

public static class EventLogger
{
    private const string SOURCE = "SyncVPN";

    public static void Initialize()
    {
        try
        {
            if (!EventLog.SourceExists(SOURCE))
            {
                EventLog.CreateEventSource(SOURCE, "Application");
            }
        }
        catch
        {
            // This process is not running as admin (and that is ok if it's the Client)
        }
    }

    public static void Log(EventLogEntryType type, string message)
    {
        Log(type, SOURCE, message);
    }

    public static void Log(EventLogEntryType type, string source, string message)
    {
        try
        {
            EventLog.WriteEntry(source, message, type);
        }
        catch // The source does not exist, which means it was not created by Initialize()
        {
            try
            {
                EventLog.WriteEntry("Application", message, type);
            }
            catch // The source "Application" does not exist. Not sure if this can ever happen, just in case
            {
            }
        }
    }
}
