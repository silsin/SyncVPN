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

using System.Drawing;
using Microsoft.UI.Xaml.Media;
using SyncVPN.Client.Common.Enums;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Core.Helpers;
using SyncVPN.Client.Core.Messages;
using SyncVPN.Client.Core.Services.Selection;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Logic.Servers.Cache;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Extensions;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.Common.Core.Helpers;
using SyncVPN.Configurations.Contracts;

namespace SyncVPN.Client.Services.Selection;

public class ApplicationIconSelector : IApplicationIconSelector,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<AuthenticationStatusChanged>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    public const string PROTON_VPN_ICON_PATH = "Assets/SyncVPN.ico";

    public static readonly ImageSource ConnectedTrayIcon = ResourceHelper.GetIcon("ProtonVpnProtectedTrayIcon");
    public static readonly ImageSource ErrorTrayIcon = ResourceHelper.GetIcon("ProtonVpnErrorTrayIcon");
    public static readonly ImageSource WarningTrayIcon = ResourceHelper.GetIcon("ProtonVpnWarningTrayIcon");
    public static readonly ImageSource DisconnectedTrayIcon = ResourceHelper.GetIcon("ProtonVpnUnprotectedTrayIcon");
    public static readonly ImageSource LoggedOutTrayIcon = ResourceHelper.GetIcon("ProtonVpnLoggedOutTrayIcon");

    public readonly Icon ConnectedBadgeIcon;
    public readonly Icon DisconnectedBadgeIcon;
    public readonly Icon ErrorBadgeIcon;
    public readonly Icon WarningBadgeIcon;

    private readonly IConfiguration _configuration;
    private readonly IServersCache _serversCache;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;

    private Severity _authenticationErrorSeverity = Severity.None;
    private Severity _connectionErrorSeverity = Severity.None;

    public AppIconStatus AppIconStatus { get; private set; } = AppIconStatus.None;

    public ApplicationIconSelector(
        IConfiguration configuration,
        IServersCache serversCache,
        IUserAuthenticator userAuthenticator,
        IConnectionManager connectionManager,
        ISettings settings,
        IEventMessageSender eventMessageSender)
    {
        _configuration = configuration;
        _serversCache = serversCache;
        _userAuthenticator = userAuthenticator;
        _connectionManager = connectionManager;
        _settings = settings;
        _eventMessageSender = eventMessageSender;

        if (OSVersion.IsWindows11OrHigher())
        {
            ConnectedBadgeIcon = GetBadgeIcon("badge-protected.ico");
            DisconnectedBadgeIcon = GetBadgeIcon("badge-unprotected.ico");
            ErrorBadgeIcon = GetBadgeIcon("badge-error.ico");
            WarningBadgeIcon = GetBadgeIcon("badge-warning.ico");
        }
        else
        {
            ConnectedBadgeIcon = GetBadgeIcon("badge-protected-w10.ico");
            DisconnectedBadgeIcon = GetBadgeIcon("badge-unprotected-w10.ico");
            ErrorBadgeIcon = GetBadgeIcon("badge-error-w10.ico");
            WarningBadgeIcon = GetBadgeIcon("badge-warning-w10.ico");
        }
    }

    public void OnAuthenticationErrorTriggered(Severity severity)
    {
        _authenticationErrorSeverity = severity;
        InvalidateAppIconStatus();
    }

    public void OnAuthenticationErrorDismissed()
    {
        _authenticationErrorSeverity = Severity.None;
        InvalidateAppIconStatus();
    }

    public void OnConnectionErrorTriggered(Severity severity)
    {
        _connectionErrorSeverity = severity;
        InvalidateAppIconStatus();
    }

    public void OnConnectionErrorDismissed()
    {
        _connectionErrorSeverity = Severity.None;
        InvalidateAppIconStatus();
    }

    public string GetAppIconPath()
    {
        return PROTON_VPN_ICON_PATH;
    }

    public ImageSource GetStatusIcon()
    {
        return AppIconStatus switch
        {
            AppIconStatus.Connected => ConnectedTrayIcon,
            AppIconStatus.Disconnected => DisconnectedTrayIcon,
            AppIconStatus.Warning => WarningTrayIcon,
            AppIconStatus.Error => ErrorTrayIcon,
            _ => LoggedOutTrayIcon
        };
    }

    public Icon? GetTaskbarBadgeIcon()
    {
        return AppIconStatus switch
        {
            AppIconStatus.Connected => ConnectedBadgeIcon,
            AppIconStatus.Disconnected => DisconnectedBadgeIcon,
            AppIconStatus.Warning => WarningBadgeIcon,
            AppIconStatus.Error => ErrorBadgeIcon,
            _ => null
        };
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        InvalidateAppIconStatus();
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        InvalidateAppIconStatus();
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName is nameof(ISettings.KillSwitchMode) or nameof(ISettings.IsKillSwitchEnabled))
        {
            InvalidateAppIconStatus();
        }
    }

    public void Receive(ServerListChangedMessage message)
    {
        InvalidateAppIconStatus();
    }

    private void InvalidateAppIconStatus()
    {
        AppIconStatus status = 
            ShouldShowConnectedStatus()
                ? AppIconStatus.Connected
                : ShouldShowErrorStatus()
                    ? AppIconStatus.Error
                    : ShouldShowWarningStatus()
                        ? AppIconStatus.Warning
                        : ShouldShowDisconnectedStatus()
                            ? AppIconStatus.Disconnected
                            : AppIconStatus.None;

        if (AppIconStatus != status)
        {
            AppIconStatus = status;
            _eventMessageSender.Send(new AppIconStatusChangedMessage(status));
        }
    }

    private bool ShouldShowConnectedStatus()
    {
        return _userAuthenticator.IsLoggedIn && _connectionManager.IsConnected;
    }

    private bool ShouldShowErrorStatus()
    {
        return _userAuthenticator.IsLoggedIn
            ? _connectionErrorSeverity is Severity.Error
            : _authenticationErrorSeverity is Severity.Error;
    }

    private bool ShouldShowWarningStatus()
    {
        if (_connectionManager.IsDisconnected && _settings.IsAdvancedKillSwitchActive())
        {
            return true;
        }

        return _userAuthenticator.IsLoggedIn
            ? _connectionErrorSeverity is Severity.Warning || _connectionManager.IsTwoFactorError || _serversCache.HasNoServers()
            : _authenticationErrorSeverity is Severity.Warning;
    }

    private bool ShouldShowDisconnectedStatus()
    {
        return _userAuthenticator.IsLoggedIn && !_connectionManager.IsConnected;
    }

    private Icon GetBadgeIcon(string iconFileName)
    {
        return new Icon(Path.Combine(_configuration.AssetsFolder, "Icons", "App", iconFileName));
    }
}