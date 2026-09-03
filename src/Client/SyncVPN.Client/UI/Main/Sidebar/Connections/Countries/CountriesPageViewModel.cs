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
using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.Common.UI.Assets.Icons.Base;
using SyncVPN.Client.Common.UI.Assets.Icons.PathIcons;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Enums;
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
    [ObservableProperty]
    private ICountriesComponent _selectedCountriesComponent;

    public override string Header => Localizer.Get("Countries");

    public override IconElement Icon => new Earth() { Size = PathIconSize.Pixels16 };

    public override int SortIndex { get; } = 2;

    public List<ICountriesComponent> CountriesComponents { get; }

    public override bool IsAvailable => ParentViewNavigator.CanNavigateToCountriesView();

    public CountriesPageViewModel(
        IConnectionsViewNavigator parentViewNavigator,
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
        CountriesComponents = new(countriesComponents.OrderBy(p => p.SortIndex));

        _selectedCountriesComponent = CountriesComponents.First();
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
        return SelectedCountriesComponent.GetItems();
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
}