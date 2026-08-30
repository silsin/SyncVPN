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

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Common.Models;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Updates.Contracts;
using SyncVPN.Client.Services.Dispatching;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Extensions;

namespace SyncVPN.Client.Commands;

public class UpdateClientCommand : IUpdateClientCommand,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>
{
    private readonly ISettings _settings;
    private readonly ILocalizationProvider _localizer;
    private readonly IUpdatesManager _updatesManager;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IConnectionManager _connectionManager;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;

    private bool _isUpdating;

    public IAsyncRelayCommand Command { get; }

    public UpdateClientCommand(
        ISettings settings,
        ILocalizationProvider localizer,
        IUpdatesManager updatesManager,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IMainWindowActivator mainWindowActivator,
        IConnectionManager connectionManager,
        IUIThreadDispatcher uiThreadDispatcher)
    {
        _settings = settings;
        _localizer = localizer;
        _updatesManager = updatesManager;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _mainWindowActivator = mainWindowActivator;
        _connectionManager = connectionManager;
        _uiThreadDispatcher = uiThreadDispatcher;

        Command = new AsyncRelayCommand(UpdateAsync, CanUpdate);
    }

    public void Receive(ClientUpdateStateChangedMessage message)
    {
        _uiThreadDispatcher.TryEnqueue(Command.NotifyCanExecuteChanged);
    }

    public bool CanUpdate()
    {
        return _updatesManager.IsUpdateAvailable && !_isUpdating;
    }

    private async Task UpdateAsync()
    {
        if (!await IsAllowedToDisconnectAsync())
        {
            return;
        }

        try
        {
            _isUpdating = true;

            bool isToOpenOnDesktop = _mainWindowActivator.IsWindowVisible;
            await _updatesManager.UpdateAsync(isToOpenOnDesktop);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private async Task<bool> IsAllowedToDisconnectAsync()
    {
        bool isAdvancedKillSwitchEnabled = _settings.IsAdvancedKillSwitchActive();
        bool isConfirmationNeeded = isAdvancedKillSwitchEnabled || !_connectionManager.IsDisconnected;

        if (!isConfirmationNeeded)
        {
            return true;
        }

        MessageDialogParameters parameters = new()
        {
            Title = _localizer.GetFormat(_updatesManager.IsAutoUpdated
                ? "Dialogs_UpdateConfirmation_Title_Restart"
                : "Dialogs_UpdateConfirmation_Title_Install"),
            Message = _localizer.Get(isAdvancedKillSwitchEnabled
                ? "Dialogs_UpdateConfirmation_Message_KillSwitchOn"
                : "Dialogs_UpdateConfirmation_Message_KillSwitchOff"),
            PrimaryButtonText = _localizer.Get(_updatesManager.IsAutoUpdated
                ? "Common_Actions_Restart"
                : "Common_Actions_Install"),
            CloseButtonText = _localizer.Get("Common_Actions_Cancel"),
        };

        ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(parameters);
        if (result is not ContentDialogResult.Primary)
        {
            return false;
        }

        return true;
    }
}