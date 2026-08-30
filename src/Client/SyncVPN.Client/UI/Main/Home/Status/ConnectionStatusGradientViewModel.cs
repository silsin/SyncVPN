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

using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Extensions;
using SyncVPN.Client.Settings.Contracts.Messages;

namespace SyncVPN.Client.UI.Main.Home.Status;

public class ConnectionStatusGradientViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionErrorMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;

    public bool IsConnected => _connectionManager.IsConnected;

    public bool IsConnecting => _connectionManager.IsConnecting;

    public bool IsDisconnected => _connectionManager.IsDisconnected;

    public bool IsInternetUnavailable => IsDisconnected && _settings.IsAdvancedKillSwitchActive();

    public bool IsTwoFactorRequired => _connectionManager.IsTwoFactorError;

    public ConnectionStatusGradientViewModel(
        IViewModelHelper viewModelHelper,
        ISettings settings,
        IConnectionManager connectionManager)
        : base(viewModelHelper)
    {
        _settings = settings;
        _connectionManager = connectionManager;
    }

    public void Receive(ConnectionErrorMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateConnectionError);
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateConnectionStatus);
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName is nameof(ISettings.KillSwitchMode) or nameof(ISettings.IsKillSwitchEnabled))
        {
            ExecuteOnUIThread(InvalidateConnectionStatus);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateConnectionStatus();
        InvalidateConnectionError();
    }

    private void InvalidateConnectionStatus()
    {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsConnecting));
        OnPropertyChanged(nameof(IsDisconnected));
        OnPropertyChanged(nameof(IsInternetUnavailable));
        OnPropertyChanged(nameof(IsTwoFactorRequired));
    }

    private void InvalidateConnectionError()
    {
        OnPropertyChanged(nameof(IsTwoFactorRequired));
    }
}