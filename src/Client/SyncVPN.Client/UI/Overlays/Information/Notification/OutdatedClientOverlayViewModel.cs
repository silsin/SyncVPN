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
using CommunityToolkit.Mvvm.Input;
using SyncVPN.Client.Commands;
using SyncVPN.Client.Contracts.Services.Browsing;
using SyncVPN.Client.Contracts.Services.Lifecycle;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Updates.Contracts;
using SyncVPN.Update.Contracts;

namespace SyncVPN.Client.UI.Overlays.Information.Notification;

public partial class OutdatedClientOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>,
    IEventMessageReceiver<ClientUpdateStateChangedMessage>
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IUpdateClientCommand _updateClientCommand;
    private readonly IUpdatesManager _updatesManager;
    private readonly IAppExitInvoker _appExitInvoker;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCheckingForUpdate))]
    [NotifyPropertyChangedFor(nameof(CanUpdate))]
    private AppUpdateStateContract? _lastUpdateState;

    public bool IsCheckingForUpdate =>
        (LastUpdateState != null &&
         !LastUpdateState.IsReady &&
         LastUpdateState?.Status is AppUpdateStatus.Checking or AppUpdateStatus.Downloading)
        || _updatesManager.IsAutoUpdateInProgress;

    public bool CanUpdate => _updatesManager.IsUpdateAvailable;

    public OutdatedClientOverlayViewModel(
        IUrlsBrowser urlsBrowser,
        IUpdateClientCommand updateClientCommand,
        IUpdatesManager updatesManager,
        IAppExitInvoker appExitInvoker,
        IMainWindowOverlayActivator overlayActivator,
        IViewModelHelper viewModelHelper)
        : base(overlayActivator, viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _updateClientCommand = updateClientCommand;
        _updatesManager = updatesManager;
        _appExitInvoker = appExitInvoker;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        _updatesManager.CheckForUpdate(isManualCheck: true);
    }

    [RelayCommand(CanExecute = nameof(CanUpdate))]
    private async Task UpdateAsync()
    {
        if (LastUpdateState != null && LastUpdateState.IsReady)
        {
            await _updateClientCommand.Command.ExecuteAsync(null);
        }
        else
        {
            _urlsBrowser.BrowseTo(_urlsBrowser.DownloadsPage);
        }

        await ExitAsync();
    }

    [RelayCommand]
    private async Task ExitAsync()
    {
        await _appExitInvoker.ForceExitAsync();
    }

    public void Receive(ClientUpdateStateChangedMessage message)
    {
        ExecuteOnUIThread(() => LastUpdateState = message.State);
    }
}