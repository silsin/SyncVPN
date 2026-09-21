/*
 * Copyright (c) 2023 Proton AG
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
using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.Common.UI.Assets.Icons.Base;
using SyncVPN.Client.Common.UI.Assets.Icons.PathIcons;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Models.Connections;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;

namespace SyncVPN.Client.UI.Main.Sidebar.Connections.Countries;

public partial class CountriesPageViewModel : ConnectionPageViewModelBase
{
    private readonly IMainViewNavigator _mainViewNavigator;

    [ObservableProperty]
    private ICountriesComponent _selectedCountriesComponent;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public override string Header => Localizer.Get("Countries");

    public override IconElement Icon => new Earth() { Size = PathIconSize.Pixels16 };

    public override int SortIndex { get; } = 2;

    public List<ICountriesComponent> CountriesComponents { get; }

    public override bool IsAvailable => ParentViewNavigator.CanNavigateToCountriesView();

    public bool IsFreePlanBannerVisible => !Settings.VpnPlan.IsPaid;

    public CountriesPageViewModel(
        IConnectionsViewNavigator parentViewNavigator,
        IMainViewNavigator mainViewNavigator,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IConnectionGroupFactory connectionGroupFactory,
        IEnumerable<ICountriesComponent> countriesComponents,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator,
               settings,
               serversLoader,
               connectionManager,
               connectionGroupFactory,
               viewModelHelper)
    {
        _mainViewNavigator = mainViewNavigator;

        CountriesComponents = new(countriesComponents.OrderBy(p => p.SortIndex));

        _selectedCountriesComponent = CountriesComponents.First();
    }

    [RelayCommand]
    private Task NavigateBackAsync()
    {
        return _mainViewNavigator.NavigateToHomeViewAsync();
    }

    // Was opening the feature-showcase carousel (an extra "Upgrade" click away from the actual Store),
    // which read as "nothing happened" to anyone who didn't notice the carousel appear. This button is
    // labeled identically to the sidebar's free-plan-card button ("Upgrade to Premium") and should behave
    // the same way: go straight to the Store page.
    [RelayCommand]
    private Task UpgradeAsync()
    {
        return _mainViewNavigator.NavigateToStoreViewAsync();
    }

    protected override void OnLoggedIn()
    {
        base.OnLoggedIn();

        GoToCountryFeature(CountriesConnectionType.All);
    }

    public override void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        if (parameter is CountriesConnectionType connectionType)
        {
            GoToCountryFeature(connectionType);
        }
    }

    protected override IEnumerable<ConnectionItemBase> GetItems()
    {
        IEnumerable<ConnectionItemBase> items = SelectedCountriesComponent.GetItems();

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return items;
        }

        return items.Where(item =>
            item.Header.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            || item.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
    }

    private void GoToCountryFeature(CountriesConnectionType connectionType)
    {
        SelectedCountriesComponent = CountriesComponents.FirstOrDefault(c => c.ConnectionType == connectionType)
                                  ?? CountriesComponents.First();
    }

    partial void OnSelectedCountriesComponentChanged(ICountriesComponent value)
    {
        FetchItems();
    }

    partial void OnSearchTextChanged(string value)
    {
        FetchItems();
    }
}