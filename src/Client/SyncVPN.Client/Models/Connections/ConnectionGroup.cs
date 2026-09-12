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
using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.Contracts.Enums;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Extensions;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Localization.Extensions;

namespace SyncVPN.Client.Models.Connections;

public partial class ConnectionGroup : List<ConnectionItemBase>
{
    private readonly IMainWindowOverlayActivator _overlayActivator;

    public ILocalizationProvider Localizer { get; }

    public ConnectionGroupType GroupType { get; }

    public bool IsHeaderVisible { get; }

    public int ItemsCount => this.Count(item => item.IsCounted);

    public IconElement? Icon => GroupType.GetIcon();

    public string Header => Localizer.GetConnectionGroupName(GroupType, ItemsCount);

    public bool IsFreeServersGroup => GroupType == ConnectionGroupType.FreeServers;

    public bool IsPremiumLocationsGroup => GroupType == ConnectionGroupType.PremiumLocations;

    public virtual bool IsInfoButtonVisible => GroupType.IsInfoButtonVisible();

    public virtual bool IsServerLoadInfoButtonVisible => !IsInfoButtonVisible && GroupType.IsServerLoadInfoButtonVisible();

    public ConnectionGroup(
        ILocalizationProvider localizer,
        IMainWindowOverlayActivator overlayActivator,
        ConnectionGroupType groupType,
        IEnumerable<ConnectionItemBase> items,
        bool showHeader)
        : base(items)
    {
        _overlayActivator = overlayActivator;

        Localizer = localizer;

        GroupType = groupType;
        IsHeaderVisible = showHeader;
    }

    [RelayCommand(CanExecute = nameof(CanShowInfoOverlay))]
    protected Task ShowInfoOverlayAsync()
    {
        return GroupType switch
        {
            ConnectionGroupType.SecureCoreCountries or
            ConnectionGroupType.SecureCoreCountryPairs => _overlayActivator.ShowSecureCoreInfoOverlayAsync(),

            ConnectionGroupType.P2PCountries or
            ConnectionGroupType.P2PStates or
            ConnectionGroupType.P2PCities or
            ConnectionGroupType.P2PServers => _overlayActivator.ShowP2PInfoOverlayAsync(),

            ConnectionGroupType.TorCountries or
            ConnectionGroupType.TorServers => _overlayActivator.ShowTorInfoOverlayAsync(),

            ConnectionGroupType.Profiles => _overlayActivator.ShowProfileInfoOverlayAsync(),

            _ => Task.CompletedTask
        };
    }

    protected bool CanShowInfoOverlay()
    {
        return IsInfoButtonVisible;
    }

    [RelayCommand(CanExecute = nameof(CanShowServerLoadInfoOverlay))]
    protected Task ShowServerLoadInfoOverlayAsync()
    {
        return _overlayActivator.ShowServerLoadInfoOverlayAsync();
    }

    protected bool CanShowServerLoadInfoOverlay()
    {
        return IsServerLoadInfoButtonVisible;
    }
}