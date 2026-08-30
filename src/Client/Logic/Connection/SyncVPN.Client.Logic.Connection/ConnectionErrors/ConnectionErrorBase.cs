/*
 * Copyright (c) 2024 Proton AG
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

using SyncVPN.Client.Common.Enums;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;

namespace SyncVPN.Client.Logic.Connection.ConnectionErrors;

public abstract class ConnectionErrorBase : IConnectionError
{
    protected readonly ILocalizationProvider Localizer;

    public virtual Severity Severity => Severity.Error;

    public virtual string Title => Localizer.Get("Connection_Error_Title");

    public abstract string Message { get; }

    public abstract string ActionLabel { get; }

    public virtual bool IsToCloseErrorOnDisconnect => false;

    protected ConnectionErrorBase(ILocalizationProvider localizer)
    {
        Localizer = localizer;
    }

    public abstract Task ExecuteActionAsync();
}