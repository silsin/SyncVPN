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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.UI.Dialogs.Upsell.Bases;

namespace SyncVPN.Client.UI.Dialogs.Upsell;

public partial class UpsellCarouselShellViewModel : ShellViewModelBase<IUpsellCarouselWindowActivator, IUpsellCarouselViewNavigator>
{
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly IMainWindowActivator _mainWindowActivator;

    [ObservableProperty]
    private IUpsellFeaturePage? _selectedUpsellFeaturePage;

    public ObservableCollection<IUpsellFeaturePage> UpsellFeaturePages { get; }

    public override string Title => Localizer.Get("Upsell_Carousel_Title");

    private int CurrentPageIndex => SelectedUpsellFeaturePage is null
        ? 0
        : UpsellFeaturePages.IndexOf(SelectedUpsellFeaturePage);

    private int LastPageIndex => UpsellFeaturePages.Count - 1;

    public UpsellCarouselShellViewModel(
        IUpsellCarouselWindowActivator windowActivator,
        IUpsellCarouselViewNavigator childViewNavigator,
        IMainViewNavigator mainViewNavigator,
        IMainWindowActivator mainWindowActivator,
        IEnumerable<IUpsellFeaturePage> upsellFeaturePages,
        IViewModelHelper viewModelHelper)
        : base(windowActivator, childViewNavigator, viewModelHelper)
    {
        _mainViewNavigator = mainViewNavigator;
        _mainWindowActivator = mainWindowActivator;

        UpsellFeaturePages = new(upsellFeaturePages.OrderBy(p => p.SortIndex));
    }

    protected override void OnChildNavigation(NavigationEventArgs e)
    {
        base.OnChildNavigation(e);

        SelectedUpsellFeaturePage = ChildViewNavigator.GetCurrentPageContext() as IUpsellFeaturePage;
    }

    [RelayCommand]
    private void MoveToNextFeature()
    {
        int nextPageIndex = CurrentPageIndex + 1;
        if (nextPageIndex > LastPageIndex)
        {
            nextPageIndex = 0;
        }

        MoveToPage(nextPageIndex);
    }

    [RelayCommand]
    private void MoveToPreviousFeature()
    {
        int previousPageIndex = CurrentPageIndex - 1;
        if (previousPageIndex < 0)
        {
            previousPageIndex = LastPageIndex;
        }

        MoveToPage(previousPageIndex);
    }

    // Used to open the external legacy upgrade/checkout page before the in-app Store existed - now that
    // it does, "Upgrade" here should land on the same Store page as everywhere else (e.g. the sidebar's
    // free-plan card), not silently open a browser tab behind the main window.
    [RelayCommand]
    private async Task UpgradeAsync()
    {
        Hide();

        _mainWindowActivator.Activate();
        await _mainViewNavigator.NavigateToStoreViewAsync();
    }

    private void MoveToPage(int index)
    {
        if (index >= 0 && index < UpsellFeaturePages.Count)
        {
            SelectedUpsellFeaturePage = UpsellFeaturePages[index];
        }
    }

    partial void OnSelectedUpsellFeaturePageChanged(IUpsellFeaturePage? value)
    {
        if (value != null && !value.IsActive)
        {
            value.InvokeAsync();
        }
    }
}