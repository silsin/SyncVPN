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

using Microsoft.UI.Xaml.Media;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Messages;
using SyncVPN.Client.Core.Services.Selection;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Logic.Profiles.Contracts.Messages;
using SyncVPN.Client.Logic.Profiles.Contracts.Models;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;

namespace SyncVPN.Client.UI.Main.FeatureIcons;

public abstract class FeatureIconViewModelBase : ViewModelBase,
    IEventMessageReceiver<ThemeChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    protected readonly IConnectionManager ConnectionManager;
    protected readonly ISettings Settings;
    protected readonly IApplicationThemeSelector ThemeSelector;

    public ImageSource Icon => GetImageSource();

    public virtual bool IsDimmed => IsFeatureEnabled && !ConnectionManager.IsConnected;

    protected abstract bool IsFeatureEnabled { get; }

    protected IConnectionProfile? CurrentProfile => ConnectionManager.CurrentConnectionIntent as IConnectionProfile;

    protected FeatureIconViewModelBase(
        IConnectionManager connectionManager,
        ISettings settings,
        IApplicationThemeSelector themeSelector,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        ConnectionManager = connectionManager;
        Settings = settings;
        ThemeSelector = themeSelector;
    }

    public void Receive(ThemeChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(SettingChangedMessage message)
    {
        if (GetSettingsChangedForIconUpdate().Contains(message.PropertyName))
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAllProperties);
    }

    public void Receive(ProfilesChangedMessage message)
    {
        if (ConnectionManager.IsConnected)
        {
            ExecuteOnUIThread(InvalidateAllProperties);
        }
    }

    protected abstract ImageSource GetImageSource();

    protected abstract IEnumerable<string> GetSettingsChangedForIconUpdate();
}