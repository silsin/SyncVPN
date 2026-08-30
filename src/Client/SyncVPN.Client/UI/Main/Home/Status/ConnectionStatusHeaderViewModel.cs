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

using CommunityToolkit.Mvvm.ComponentModel;
using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Extensions;
using SyncVPN.Client.Settings.Contracts.Messages;

namespace SyncVPN.Client.UI.Main.Home.Status;

public partial class ConnectionStatusHeaderViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionErrorMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private const int REFRESH_TIMER_INTERVAL_IN_MS = 1000;

    private readonly IDispatcherTimer _refreshTimer;

    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProtectionDescription))]
    private TimeSpan _sessionLength = TimeSpan.Zero;

    public bool IsConnected => _connectionManager.IsConnected;

    public bool IsConnecting => _connectionManager.IsConnecting;

    public bool IsDisconnected => _connectionManager.IsDisconnected;

    public bool IsInternetUnavailable => IsDisconnected && _settings.IsAdvancedKillSwitchActive();

    public bool IsTwoFactorRequired => _connectionManager.IsTwoFactorError;

    public string ProtectionTitle =>
        _connectionManager.ConnectionStatus switch
        {
            ConnectionStatus.Disconnected => _settings.IsAdvancedKillSwitchActive()
                ? Localizer.Get("Home_ConnectionDetails_AdvancedKillSwitchActivated")
                : Localizer.Get("Home_ConnectionDetails_Unprotected"),
            ConnectionStatus.Connecting => _connectionManager.IsTwoFactorError
                ? Localizer.Get("Home_ConnectionDetails_TwoFactorRequired_Title")
                : Localizer.Get("Home_ConnectionDetails_Connecting"),
            ConnectionStatus.Connected => Localizer.Get("Home_ConnectionDetails_Protected"),
            _ => string.Empty,
        };

    public string ProtectionDescription =>
        _connectionManager.ConnectionStatus switch
        {
            ConnectionStatus.Disconnected => _settings.IsAdvancedKillSwitchActive()
                ? Localizer.Get("Home_ConnectionDetails_ConnectToRestoreConnection")
                : Localizer.Get("Home_ConnectionDetails_UnprotectedSubLabel"),
            ConnectionStatus.Connecting => _connectionManager.IsTwoFactorError
                ? Localizer.Get("Home_ConnectionDetails_TwoFactorRequired_Description")
                : Localizer.Get("Home_ConnectionDetails_ConnectingSubLabel"),
            ConnectionStatus.Connected => Localizer.GetFormattedTime(SessionLength) ?? string.Empty,
            _ => string.Empty,
        };

    public ConnectionStatusHeaderViewModel(
        IConnectionManager connectionManager,
        ISettings settings,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _connectionManager = connectionManager;
        _settings = settings;

        _refreshTimer = UIThreadDispatcher.GetTimer(TimeSpan.FromMilliseconds(REFRESH_TIMER_INTERVAL_IN_MS));
        _refreshTimer.Tick += OnRefreshTimerTick;
    }

    public void Receive(ConnectionErrorMessage message)
    {
        ExecuteOnUIThread(InvalidateConnectionError);
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateConnectionStatus);
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

        InvalidateAutoRefreshTimer();
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        InvalidateAutoRefreshTimer();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(ProtectionTitle));
        OnPropertyChanged(nameof(ProtectionDescription));
    }

    private void InvalidateConnectionError()
    {
        OnPropertyChanged(nameof(IsTwoFactorRequired));
        OnPropertyChanged(nameof(ProtectionTitle));
        OnPropertyChanged(nameof(ProtectionDescription));
    }

    private void InvalidateConnectionStatus()
    {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsConnecting));
        OnPropertyChanged(nameof(IsDisconnected));
        OnPropertyChanged(nameof(IsInternetUnavailable));
        OnPropertyChanged(nameof(IsTwoFactorRequired));
        OnPropertyChanged(nameof(ProtectionTitle));
        OnPropertyChanged(nameof(ProtectionDescription));

        InvalidateAutoRefreshTimer();
    }

    private void InvalidateAutoRefreshTimer()
    {
        if (_connectionManager.IsConnected)
        {
            if (!_refreshTimer.IsEnabled)
            {
                InvalidateSessionLength();
                _refreshTimer.Start();
            }
        }
        else
        {
            if (_refreshTimer.IsEnabled)
            {
                _refreshTimer.Stop();
            }
        }
    }

    private void OnRefreshTimerTick(object? sender, object e)
    {
        InvalidateSessionLength();
    }

    private void InvalidateSessionLength()
    {
        ConnectionDetails? connectionDetails = _connectionManager.CurrentConnectionDetails;
        SessionLength = connectionDetails?.EstablishedConnectionTimeUtc is null
            ? TimeSpan.Zero
            : DateTime.UtcNow - connectionDetails.EstablishedConnectionTimeUtc.Value;
    }
}