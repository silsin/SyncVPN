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

using SyncVPN.Client.Contracts.Profiles;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Profiles.Contracts.Models;

namespace SyncVPN.Client.Logic.Connection.ConnectionErrors;

public class NoServersForProfileConnectionError : ConnectionErrorBase
{
    private readonly IProfileEditor _profileEditor;
    private readonly IConnectionManager _connectionManager;

    public override string Message => Localizer.GetFormat("Connection_Error_NoServers_Profile", ConnectionProfile?.Name ?? string.Empty);

    public override string ActionLabel => Localizer.Get("Connection_Error_EditProfile");

    private IConnectionProfile? ConnectionProfile => _connectionManager.CurrentConnectionIntent as IConnectionProfile;

    public NoServersForProfileConnectionError(
        ILocalizationProvider localizer,
        IProfileEditor profileEditor,
        IConnectionManager connectionManager)
        : base(localizer)
    {
        _profileEditor = profileEditor;
        _connectionManager = connectionManager;
    }

    public override async Task ExecuteActionAsync()
    {
        IConnectionProfile? connectionProfile = ConnectionProfile;
        if (connectionProfile is not null)
        {
            await _profileEditor.EditProfileAsync(connectionProfile);
        }
    }
}