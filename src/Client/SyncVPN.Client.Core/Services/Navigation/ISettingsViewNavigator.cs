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

using System.Threading.Tasks;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Core.Services.Navigation.Bases;

namespace SyncVPN.Client.Core.Services.Navigation;

public interface ISettingsViewNavigator : IViewNavigator
{
    Task<bool> NavigateToFeatureViewAsync(ConnectionFeature feature, bool isDirectNavigation = false);

    Task<bool> NavigateToCommonSettingsViewAsync(bool forceNavigation = false);

    Task<bool> NavigateToAdvancedSettingsViewAsync();

    Task<bool> NavigateToConnectionPreferencesSettingsViewAsync(bool isDirectNavigation = false);

    Task<bool> NavigateToProtocolSettingsViewAsync(bool isDirectNavigation = false);

    Task<bool> NavigateToNetShieldSettingsViewAsync(bool isDirectNavigation = false);

    Task<bool> NavigateToKillSwitchSettingsViewAsync(bool isDirectNavigation = false);

    Task<bool> NavigateToPortForwardingSettingsViewAsync(bool isDirectNavigation = false);

    Task<bool> NavigateToSplitTunnelingSettingsViewAsync(bool isDirectNavigation = false);

    Task<bool> NavigateToVpnAcceleratorSettingsViewAsync();

    Task<bool> NavigateToCustomDnsSettingsViewAsync();

    Task<bool> NavigateToAutoStartupSettingsViewAsync();

    Task<bool> NavigateToDebugLogsSettingsViewAsync();

    Task<bool> NavigateToAboutViewAsync();

    Task<bool> NavigateToCensorshipViewAsync();

    Task<bool> NavigateToLicensingViewAsync();
}