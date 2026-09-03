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

using Microsoft.UI.Xaml.Navigation;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Messages;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.UI.Main.Profiles;
using SyncVPN.Client.UI.Main.Settings;

namespace SyncVPN.Client.UI.Main;

public partial class MainPageViewModel : PageViewModelBase<IMainWindowViewNavigator, IMainViewNavigator>
{
    // Fixed sidebar width, kept in sync with SidebarComponentView.xaml's root Border Width.
    private const double SIDEBAR_TOTAL_WIDTH = 230;

    private readonly IEventMessageSender _eventMessageSender;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly IUserAuthenticator _userAuthenticator;

    public double EffectiveSidebarWidth => SIDEBAR_TOTAL_WIDTH;

    public bool IsHomePageDisplayed => ChildViewNavigator.GetCurrentPageContext() is null;

    public MainPageViewModel(
        IEventMessageSender eventMessageSender,
        IMainWindowViewNavigator parentViewNavigator,
        IMainViewNavigator childViewNavigator,
        IMainWindowActivator mainWindowActivator,
        IUserAuthenticator userAuthenticator,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, childViewNavigator, viewModelHelper)
    {
        _eventMessageSender = eventMessageSender;
        _mainWindowActivator = mainWindowActivator;
        _userAuthenticator = userAuthenticator;
    }

    public async Task CloseCurrentPageAsync()
    {
        if (IsHomePageDisplayed)
        {
            return;
        }

        switch (ChildViewNavigator.GetCurrentPageContext())
        {
            case SettingsPageViewModel settingsPage:
                await settingsPage.CloseAsync();
                break;
            case ProfilePageViewModel profilePage:
                await profilePage.CloseAsync();
                break;
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateWindowChrome();

        _eventMessageSender.Send<HomePageDisplayedAfterLoginMessage>();
    }

    // This page is only ever shown once logged in, so the title bar (and, gated by
    // it, the min/max/resize caption buttons) should always be visible here. This
    // self-heals the case where MainWindowActivator.OnInitialized() ran its own
    // title-bar-visibility check before IUserAuthenticator.IsLoggedIn had resolved
    // to true yet (e.g. a restored/cached session with no fresh login transition
    // event to trigger a later re-check).
    private void InvalidateWindowChrome()
    {
        if (_mainWindowActivator.Window is MainWindow mainWindow)
        {
            mainWindow.InvalidateTitleBarVisibility(isTitleBarVisible: true);
        }
    }

    protected override void OnChildNavigation(NavigationEventArgs e)
    {
        base.OnChildNavigation(e);

        OnPropertyChanged(nameof(IsHomePageDisplayed));

        if (IsHomePageDisplayed)
        {
            InvalidateWindowChrome();

            _eventMessageSender.Send<NavigatedToHomePageMessage>();
        }
    }
}