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

using Microsoft.UI.Xaml;
using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Common.Messages;
using SyncVPN.Client.Common.UI.Controls.Custom;
using SyncVPN.Client.Contracts.Messages;
using SyncVPN.Client.Core.Extensions;
using SyncVPN.Client.Core.Services.Selection;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Logging.Contracts;

namespace SyncVPN.Client.Core.Services.Activation.Bases;

public abstract class DialogActivatorBase<TWindow> : WindowActivatorBase<TWindow>,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>,
    IEventMessageReceiver<ApplicationStoppedMessage>
    where TWindow : BaseWindow
{
    protected readonly IMainWindowActivator MainWindowActivator;

    private bool _isHiddenAfterMainWindowClosed;

    protected DialogActivatorBase(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationService localizationService,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IMainWindowActivator mainWindowActivator)
        : base(logger,
               uiThreadDispatcher,
               themeSelector,
               settings,
               localizationService,
               localizer,
               iconSelector)
    {
        MainWindowActivator = mainWindowActivator;
        EnableExitOnEsc = true;
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        if (Host == null)
        {
            return;
        }

        UIThreadDispatcher.TryEnqueue(() =>
        {
            if (message.IsMainWindowVisible)
            {
                if (_isHiddenAfterMainWindowClosed)
                {
                    Activate();
                }
            }
            else if (Host.Visible)
            {
                _isHiddenAfterMainWindowClosed = true;
                Hide();
            }
        });
    }

    public void Receive(ApplicationStoppedMessage message)
    {
        UIThreadDispatcher.TryEnqueue(Exit);
    }

    protected override void InvalidateWindowPosition()
    {
        if (MainWindowActivator.Window != null)
        {
            Host?.CenterOnMainWindowMonitor(MainWindowActivator.Window);
            return;
        }

        base.InvalidateWindowPosition();
    }

    protected override void OnWindowOpened()
    {
        base.OnWindowOpened();

        _isHiddenAfterMainWindowClosed = false;
    }

    protected override void OnWindowClosing(WindowEventArgs e)
    {
        base.OnWindowClosing(e);

        e.Handled = true;
        Hide();
    }
}