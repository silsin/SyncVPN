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

using CommunityToolkit.Mvvm.Input;
using SyncVPN.Client.Contracts.Services.Lifecycle;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Navigation;

namespace SyncVPN.Client.UI.Dialogs.Tray;

public partial class TrayAppShellViewModel : ShellViewModelBase<ITrayAppWindowActivator, ITrayAppViewNavigator>
{
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IAppExitInvoker _appExitInvoker;

    public string ApplicationName { get; } = App.APPLICATION_NAME;

    public string OpenApplicationLabel => Localizer.GetFormat("Tray_Actions_OpenApplication", ApplicationName);

    public TrayAppShellViewModel(
        ITrayAppWindowActivator windowActivator,
        ITrayAppViewNavigator childViewNavigator,
        IViewModelHelper viewModelHelper,
        IMainWindowActivator mainWindowActivator,
        IAppExitInvoker appExitInvoker)
        : base(windowActivator,
               childViewNavigator,
               viewModelHelper)
    {
        _mainWindowActivator = mainWindowActivator;
        _appExitInvoker = appExitInvoker;
    }

    [RelayCommand]
    public void ShowApplication()
    {
        _mainWindowActivator.Activate();
    }

    [RelayCommand]
    public Task ExitApplicationAsync()
    {
        return _appExitInvoker.ExitWithConfirmationAsync();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(OpenApplicationLabel));
    }
}