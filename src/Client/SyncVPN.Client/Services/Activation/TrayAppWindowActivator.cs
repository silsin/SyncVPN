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

using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Activation.Bases;
using SyncVPN.Client.Core.Services.Selection;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Dialogs.Tray;
using SyncVPN.Logging.Contracts;
using SyncVPN.Client.Core.Extensions;
using SyncVPN.Client.Logic.Auth.Contracts;

namespace SyncVPN.Client.Services.Activation;

public class TrayAppWindowActivator : DialogActivatorBase<TrayAppWindow>, ITrayAppWindowActivator
{
    private const double TRAY_APP_DEFAULT_WIDTH = 425;
    private const double TRAY_APP_DEFAULT_HEIGHT = 620;
    private const double TRAY_APP_FREE_USER_HEIGHT = 520;
    private const double TRAY_APP_MARGIN = 10;

    private readonly IUserAuthenticator _userAuthenticator;

    public override string WindowTitle { get; } = $"{App.APPLICATION_NAME} (tray)";

    protected override bool ShouldCreateInstanceIfMissing => false;

    public TrayAppWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationService localizationService,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IMainWindowActivator mainWindowActivator,
        IUserAuthenticator userAuthenticator)
        : base(logger,
               uiThreadDispatcher,
               themeSelector,
               settings,
               localizationService,
               localizer,
               iconSelector,
               mainWindowActivator)
    {
        _userAuthenticator = userAuthenticator;
    }

    protected override void InvalidateWindowPosition()
    {
        double height = _userAuthenticator.IsLoggedIn && !Settings.VpnPlan.IsPaid
            ? TRAY_APP_FREE_USER_HEIGHT
            : TRAY_APP_DEFAULT_HEIGHT;

        Host?.MoveNearTray(TRAY_APP_DEFAULT_WIDTH, height, TRAY_APP_MARGIN);
    }

    protected override void OnWindowFocused()
    {
        base.OnWindowFocused();

        InvalidateWindowPosition();
    }

    protected override void OnWindowUnfocused()
    {
        base.OnWindowUnfocused();

        Hide();
    }
}