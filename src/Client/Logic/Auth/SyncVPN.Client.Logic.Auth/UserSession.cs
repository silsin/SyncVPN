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

using SyncVPN.Api.Contracts;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Enums;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;

namespace SyncVPN.Client.Logic.Auth;

public class UserSession : IUserSession,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    private AuthenticationStatus _authenticationStatus;

    public bool IsLoggedIn => _authenticationStatus == AuthenticationStatus.LoggedIn;

    public void Receive(AuthenticationStatusChanged message)
    {
        _authenticationStatus = message.AuthenticationStatus;
    }
}