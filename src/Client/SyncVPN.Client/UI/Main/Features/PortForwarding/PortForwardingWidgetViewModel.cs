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
using SyncVPN.Client.Contracts.Profiles;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.RequiredReconnections;
using SyncVPN.Client.UI.Main.Features.Bases;
using SyncVPN.Client.UI.Main.Settings.Bases;
using SyncVPN.Client.UI.Main.Settings.Connection;

namespace SyncVPN.Client.UI.Main.Features.PortForwarding;

public partial class PortForwardingWidgetViewModel : FeatureWidgetViewModelBase,
    IEventMessageReceiver<PortForwardingStatusChangedMessage>,
    IEventMessageReceiver<PortForwardingPortChangedMessage>
{
    private readonly IPortForwardingManager _portForwardingManager;
    private readonly Lazy<List<ChangedSettingArgs>> _disablePortForwardingSettings;
    private readonly Lazy<List<ChangedSettingArgs>> _enablePortForwardingSettings;

    public override string Header => Localizer.Get("Settings_Connection_PortForwarding");

    public string InfoMessage => ConnectionManager.IsConnected
                              && IsPortForwardingEnabled
                              && _portForwardingManager.ActivePort is not null
        ? Localizer.Get("Flyouts_PortForwarding_ActivePort_Info")
        : Localizer.Get("Flyouts_PortForwarding_Info");

    public string WarningMessage => !ConnectionManager.IsP2PServerConnection()
        ? Localizer.Get("Flyouts_PortForwarding_Warning")
        : Localizer.Get("Flyouts_PortForwarding_ActivePort_Error");

    public bool IsPortForwardingEnabled => IsFeatureOverridden
        ? CurrentProfile!.Settings.IsPortForwardingEnabled
        : Settings.IsPortForwardingEnabled;

    public bool IsInfoMessageVisible => !ConnectionManager.IsConnected
                                     || !IsPortForwardingEnabled
                                     || (ConnectionManager.IsP2PServerConnection() && !_portForwardingManager.HasError);

    public bool IsWarningMessageVisible => ConnectionManager.IsConnected
                                        && IsPortForwardingEnabled
                                        && (!ConnectionManager.IsP2PServerConnection() || _portForwardingManager.HasError);

    public bool IsActivePortComponentVisible => ConnectionManager.IsConnected
                                             && IsPortForwardingEnabled;

    protected override UpsellFeatureType? UpsellFeature { get; } = UpsellFeatureType.P2P;

    public override bool IsFeatureOverridden => ConnectionManager.IsConnected
                                             && CurrentProfile != null;

    public PortForwardingWidgetViewModel(
        IViewModelHelper viewModelHelper,
        ISettings settings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        ISettingsConflictResolver settingsConflictResolver,
        IProfileEditor profileEditor,
        IPortForwardingManager portForwardingManager)
        : base(viewModelHelper,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               connectionManager,
               upsellCarouselWindowActivator,
               requiredReconnectionSettings,
               settingsConflictResolver,
               profileEditor,
               ConnectionFeature.PortForwarding)
    {
        _portForwardingManager = portForwardingManager;

        _disablePortForwardingSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.IsPortForwardingEnabled, () => false)
        ]);

        _enablePortForwardingSettings = new(() =>
        [
            ChangedSettingArgs.Create(() => Settings.IsPortForwardingEnabled, () => true)
        ]);
    }

    protected override IEnumerable<string> GetSettingsChangedForUpdate()
    {
        yield return nameof(ISettings.IsPortForwardingEnabled);
    }

    protected override string GetFeatureStatus()
    {
        return Localizer.GetToggleValue(IsPortForwardingEnabled);
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(WarningMessage));
    }

    protected override void OnSettingsChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsWarningMessageVisible));
        OnPropertyChanged(nameof(IsActivePortComponentVisible));
        OnPropertyChanged(nameof(IsPortForwardingEnabled));
    }

    protected override void OnConnectionStatusChanged()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(WarningMessage));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsWarningMessageVisible));
        OnPropertyChanged(nameof(IsActivePortComponentVisible));
        OnPropertyChanged(nameof(IsFeatureOverridden));
        OnPropertyChanged(nameof(CurrentProfile));
        OnPropertyChanged(nameof(IsPortForwardingEnabled));
    }

    public void Receive(PortForwardingStatusChangedMessage message)
    {
        ExecuteOnUIThread(OnPortForwardingChange);
    }

    private void OnPortForwardingChange()
    {
        OnPropertyChanged(nameof(InfoMessage));
        OnPropertyChanged(nameof(IsInfoMessageVisible));
        OnPropertyChanged(nameof(IsWarningMessageVisible));
    }

    public void Receive(PortForwardingPortChangedMessage message)
    {
        ExecuteOnUIThread(OnPortForwardingChange);
    }

    protected override bool IsOnFeaturePage(PageViewModelBase? currentPageContext)
    {
        return currentPageContext is PortForwardingPageViewModel;
    }

    [RelayCommand]
    private Task<bool> DisablePortForwardingAsync()
    {
        return TryChangeFeatureSettingsAsync(_disablePortForwardingSettings.Value);
    }

    [RelayCommand]
    private Task<bool> EnablePortForwardingAsync()
    {
        return TryChangeFeatureSettingsAsync(_enablePortForwardingSettings.Value);
    }
}