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

using CommunityToolkit.Mvvm.ComponentModel;
using SyncVPN.Client.Common.Enums;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Logic.Profiles.Contracts.Messages;
using SyncVPN.Client.Logic.Profiles.Contracts.Models;

namespace SyncVPN.Client.UI.Main.Components;

public partial class CurrentProfileIconViewModel : ViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FlagType))]
    [NotifyPropertyChangedFor(nameof(ExitCountry))]
    private IConnectionProfile? _currentProfile;

    public FlagType FlagType => CurrentProfile?.Location.GetFlagType(_connectionManager.IsConnected) ?? FlagType.Fastest;

    public string ExitCountry => CurrentProfile?.Location.GetCountryCode() ?? string.Empty;

    public CurrentProfileIconViewModel(
            IViewModelHelper viewModelHelper,
            IConnectionManager connectionManager)
            : base(viewModelHelper)
    {
        _connectionManager = connectionManager;

        InvalidateCurrentProfile();
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateCurrentProfile);
    }

    public void Receive(ProfilesChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateCurrentProfile);
    }

    private void InvalidateCurrentProfile()
    {
        CurrentProfile = _connectionManager.CurrentConnectionIntent as IConnectionProfile;
    }
}