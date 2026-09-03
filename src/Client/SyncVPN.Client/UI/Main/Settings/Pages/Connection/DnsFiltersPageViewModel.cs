/*
 * Copyright (c) 2026 Proton AG
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts.Dns;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Services.DnsFilters;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.RequiredReconnections;
using SyncVPN.Client.UI.Main.Settings.Bases;

namespace SyncVPN.Client.UI.Main.Settings.Pages.Connection;

// A net-new settings page, not a migration of anything existing - see the migration plan's Phase 3.
// Unlike other SettingsPageViewModelBase pages, the "settings" here aren't local ISettings values:
// they're fetched from and written back to the new SyncVPN backend, so OnRetrieveSettingsAsync/
// OnSaveSettingsAsync (async hooks the base class already provides) do the real work, while the
// Baseline*Enabled properties exist purely so ChangedSettingArgs/PageSettings can still drive the
// existing Apply-button dirty-tracking without needing a real ISettings-backed field.
public partial class DnsFiltersPageViewModel : SettingsPageViewModelBase
{
    private readonly IDnsFiltersManager _dnsFiltersManager;

    private DnsFilterStates? _lastKnownFilters;

    [ObservableProperty]
    private bool _isMalwareFilterEnabled;

    [ObservableProperty]
    private bool _isAdsTrackersFilterEnabled;

    [ObservableProperty]
    private bool _isSocialNetworksFilterEnabled;

    [ObservableProperty]
    private bool _isPornFilterEnabled;

    [ObservableProperty]
    private bool _isGamblingFilterEnabled;

    [ObservableProperty]
    private bool _isClickbaitFilterEnabled;

    [ObservableProperty]
    private bool _isOtherVpnsFilterEnabled;

    [ObservableProperty]
    private bool _isCryptoFilterEnabled;

    [ObservableProperty]
    private bool _canEditDnsFilters;

    [ObservableProperty]
    private bool _isLoadFailed;

    private bool BaselineMalwareEnabled { get; set; }
    private bool BaselineAdsTrackersEnabled { get; set; }
    private bool BaselineSocialNetworksEnabled { get; set; }
    private bool BaselinePornEnabled { get; set; }
    private bool BaselineGamblingEnabled { get; set; }
    private bool BaselineClickbaitEnabled { get; set; }
    private bool BaselineOtherVpnsEnabled { get; set; }
    private bool BaselineCryptoEnabled { get; set; }

    public override string Title => Localizer.Get("Settings_Connection_DnsFilters");

    public DnsFiltersPageViewModel(
        IDnsFiltersManager dnsFiltersManager,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IViewModelHelper viewModelHelper)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               viewModelHelper)
    {
        _dnsFiltersManager = dnsFiltersManager;

        PageSettings =
        [
            ChangedSettingArgs.Create(() => BaselineMalwareEnabled, () => IsMalwareFilterEnabled),
            ChangedSettingArgs.Create(() => BaselineAdsTrackersEnabled, () => IsAdsTrackersFilterEnabled),
            ChangedSettingArgs.Create(() => BaselineSocialNetworksEnabled, () => IsSocialNetworksFilterEnabled),
            ChangedSettingArgs.Create(() => BaselinePornEnabled, () => IsPornFilterEnabled),
            ChangedSettingArgs.Create(() => BaselineGamblingEnabled, () => IsGamblingFilterEnabled),
            ChangedSettingArgs.Create(() => BaselineClickbaitEnabled, () => IsClickbaitFilterEnabled),
            ChangedSettingArgs.Create(() => BaselineOtherVpnsEnabled, () => IsOtherVpnsFilterEnabled),
            ChangedSettingArgs.Create(() => BaselineCryptoEnabled, () => IsCryptoFilterEnabled),
        ];
    }

    // Not reconnection-relevant - these are server-side account settings, unrelated to the tunnel.
    protected override bool IsReconnectionRequiredDueToChanges(IEnumerable<ChangedSettingArgs> changedSettings)
    {
        return false;
    }

    protected override async Task OnRetrieveSettingsAsync()
    {
        ApiResponseResult<AccountDnsFilterResponse> response = await _dnsFiltersManager.GetDnsFiltersAsync();

        IsLoadFailed = !response.Success;
        if (!response.Success)
        {
            return;
        }

        AccountDnsFilterData data = response.Value.Data;
        _lastKnownFilters = data.Filters;
        CanEditDnsFilters = data.CanUpdate;

        IsMalwareFilterEnabled = BaselineMalwareEnabled = IsOn(data.Filters.Malware);
        IsAdsTrackersFilterEnabled = BaselineAdsTrackersEnabled = IsOn(data.Filters.AdsTrackers);
        IsSocialNetworksFilterEnabled = BaselineSocialNetworksEnabled = IsOn(data.Filters.SocialNetworks);
        IsPornFilterEnabled = BaselinePornEnabled = IsOn(data.Filters.Porn);
        IsGamblingFilterEnabled = BaselineGamblingEnabled = IsOn(data.Filters.Gambling);
        IsClickbaitFilterEnabled = BaselineClickbaitEnabled = IsOn(data.Filters.Clickbait);
        IsOtherVpnsFilterEnabled = BaselineOtherVpnsEnabled = IsOn(data.Filters.OtherVpns);
        IsCryptoFilterEnabled = BaselineCryptoEnabled = IsOn(data.Filters.Crypto);
    }

    protected override async Task OnSaveSettingsAsync()
    {
        if (_lastKnownFilters is null)
        {
            return;
        }

        DnsFilterPatch patch = new();
        AddIfChanged(_lastKnownFilters.Malware, IsMalwareFilterEnabled, v => patch.Malware = v);
        AddIfChanged(_lastKnownFilters.AdsTrackers, IsAdsTrackersFilterEnabled, v => patch.AdsTrackers = v);
        AddIfChanged(_lastKnownFilters.SocialNetworks, IsSocialNetworksFilterEnabled, v => patch.SocialNetworks = v);
        AddIfChanged(_lastKnownFilters.Porn, IsPornFilterEnabled, v => patch.Porn = v);
        AddIfChanged(_lastKnownFilters.Gambling, IsGamblingFilterEnabled, v => patch.Gambling = v);
        AddIfChanged(_lastKnownFilters.Clickbait, IsClickbaitFilterEnabled, v => patch.Clickbait = v);
        AddIfChanged(_lastKnownFilters.OtherVpns, IsOtherVpnsFilterEnabled, v => patch.OtherVpns = v);
        AddIfChanged(_lastKnownFilters.Crypto, IsCryptoFilterEnabled, v => patch.Crypto = v);

        if (!HasAnyField(patch))
        {
            return;
        }

        ApiResponseResult<UpdateAccountDnsFiltersResponse> response = await _dnsFiltersManager.UpdateDnsFiltersAsync(patch);
        if (response.Success)
        {
            _lastKnownFilters = response.Value.Data.Filters;
        }
        // On failure the toggles keep the user's chosen values but nothing was persisted server-side -
        // SettingsPageViewModelBase has no "save failed, stay on page" surface today, so this is a known
        // gap rather than a design choice; acceptable for a first pass at a non-critical, reversible setting.
    }

    private static bool IsOn(string state) => state == "on";

    private static string ToOnOff(bool value) => value ? "on" : "off";

    private static void AddIfChanged(string previousState, bool currentValue, Action<string> setPatchField)
    {
        if (IsOn(previousState) != currentValue)
        {
            setPatchField(ToOnOff(currentValue));
        }
    }

    private static bool HasAnyField(DnsFilterPatch patch)
    {
        return patch.Malware is not null
            || patch.AdsTrackers is not null
            || patch.SocialNetworks is not null
            || patch.Porn is not null
            || patch.Gambling is not null
            || patch.Clickbait is not null
            || patch.OtherVpns is not null
            || patch.Crypto is not null;
    }
}
