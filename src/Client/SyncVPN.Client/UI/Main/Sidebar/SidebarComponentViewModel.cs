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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Main.Settings;
using SyncVPN.Client.UI.Main.Settings.Connection;
using SyncVPN.Client.UI.Main.Sidebar.Connections;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Countries;

namespace SyncVPN.Client.UI.Main.Sidebar;

public partial class SidebarComponentViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly IConnectionsViewNavigator _connectionsViewNavigator;
    private readonly ISettingsViewNavigator _settingsViewNavigator;
    private readonly ISettings _settings;
    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHomeSelected))]
    [NotifyPropertyChangedFor(nameof(IsCountriesSelected))]
    [NotifyPropertyChangedFor(nameof(IsSecureCoreSelected))]
    [NotifyPropertyChangedFor(nameof(IsNetShieldSelected))]
    [NotifyPropertyChangedFor(nameof(IsSettingsSelected))]
    private SidebarSection _selectedSection = SidebarSection.Home;

    [ObservableProperty]
    private string _planLabel = string.Empty;

    [ObservableProperty]
    private bool _isFreePlan;

    public bool IsHomeSelected => SelectedSection == SidebarSection.Home;

    public bool IsCountriesSelected => SelectedSection == SidebarSection.Countries;

    public bool IsSecureCoreSelected => SelectedSection == SidebarSection.SecureCore;

    public bool IsNetShieldSelected => SelectedSection == SidebarSection.NetShield;

    public bool IsSettingsSelected => SelectedSection == SidebarSection.Settings;

    // The following are static placeholders: this app has no daily-data-quota
    // tracking backend, so these values aren't wired to any real usage data.
    public string DailyDataUsageText => "512 MB / 512 MB";

    public double DailyDataUsageValue => 512;

    public double DailyDataUsageMaximum => 512;

    public string DailyDataUsedText => "0%";

    public string ResetsInText => "23:59:59";

    public SidebarComponentViewModel(
        IMainViewNavigator mainViewNavigator,
        IConnectionsViewNavigator connectionsViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        ISettings settings,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _mainViewNavigator = mainViewNavigator;
        _connectionsViewNavigator = connectionsViewNavigator;
        _settingsViewNavigator = settingsViewNavigator;
        _settings = settings;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;

        _mainViewNavigator.Navigated += OnMainNavigated;
        _settingsViewNavigator.Navigated += OnSettingsNavigated;

        InvalidatePlan();
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(InvalidatePlan);
    }

    [RelayCommand]
    private async Task NavigateHomeAsync()
    {
        SelectedSection = SidebarSection.Home;
        await _mainViewNavigator.NavigateToHomeViewAsync();
    }

    [RelayCommand]
    private async Task NavigateCountriesAsync()
    {
        SelectedSection = SidebarSection.Countries;
        await _mainViewNavigator.NavigateToCountriesViewAsync();
    }

    [RelayCommand]
    private async Task NavigateSecureCoreAsync()
    {
        SelectedSection = SidebarSection.SecureCore;
        await _mainViewNavigator.NavigateToSecureCoreViewAsync();
    }

    [RelayCommand]
    private async Task NavigateNetShieldAsync()
    {
        SelectedSection = SidebarSection.NetShield;
        await _mainViewNavigator.NavigateToSettingsViewAsync();
        await _settingsViewNavigator.NavigateToNetShieldSettingsViewAsync();
    }

    [RelayCommand]
    private async Task NavigateSettingsAsync()
    {
        SelectedSection = SidebarSection.Settings;
        await _mainViewNavigator.NavigateToSettingsViewAsync();
        await _settingsViewNavigator.NavigateToCommonSettingsViewAsync();
    }

    [RelayCommand]
    private Task UpgradeToPremiumAsync()
    {
        return _upsellCarouselWindowActivator.ActivateAsync(UpsellFeatureType.WorldwideCoverage);
    }

    private void OnMainNavigated(object sender, NavigationEventArgs e)
    {
        switch (_mainViewNavigator.GetCurrentPageContext())
        {
            case ConnectionsPageViewModel:
                InvalidateConnectionsSection();
                break;
            case SettingsPageViewModel:
                InvalidateSettingsSection();
                break;
            case null:
                SelectedSection = SidebarSection.Home;
                break;
        }
    }

    private void OnSettingsNavigated(object sender, NavigationEventArgs e)
    {
        if (_mainViewNavigator.GetCurrentPageContext() is SettingsPageViewModel)
        {
            InvalidateSettingsSection();
        }
    }

    private void InvalidateConnectionsSection()
    {
        SelectedSection = _connectionsViewNavigator.GetCurrentPageContext() switch
        {
            CountriesPageViewModel countries when countries.SelectedCountriesComponent.ConnectionType == CountriesConnectionType.SecureCore => SidebarSection.SecureCore,
            _ => SidebarSection.Countries
        };
    }

    private void InvalidateSettingsSection()
    {
        SelectedSection = _settingsViewNavigator.GetCurrentPageContext() switch
        {
            NetShieldPageViewModel => SidebarSection.NetShield,
            _ => SidebarSection.Settings
        };
    }

    private void InvalidatePlan()
    {
        PlanLabel = _settings.VpnPlan.IsFreePlan || _settings.VpnPlan.IsDefaultPlan
            ? Localizer.Get("Sidebar_FreePlanBadge")
            : _settings.VpnPlan.Title;
        IsFreePlan = _settings.VpnPlan.IsFreePlan || _settings.VpnPlan.IsDefaultPlan;
    }
}
