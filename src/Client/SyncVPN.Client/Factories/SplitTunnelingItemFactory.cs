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

using Microsoft.UI.Xaml.Media;
using SyncVPN.Client.Contracts.Enums;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Models.Features.SplitTunneling;
using SyncVPN.Client.Settings.Contracts.Enums;
using SyncVPN.Client.Settings.Contracts.Models;
using SyncVPN.Client.Common.UI.Extensions;

namespace SyncVPN.Client.Factories;

public class SplitTunnelingItemFactory : ISplitTunnelingItemFactory
{
    private readonly ILocalizationProvider _localizer;

    public SplitTunnelingItemFactory(
        ILocalizationProvider localizer)
    {
        _localizer = localizer;
    }

    public SplitTunnelingGroup GetGroup(SplitTunnelingGroupType groupType, IEnumerable<SplitTunnelingItemBase> items)
    {
        return new SplitTunnelingGroup(_localizer, groupType, items);
    }

    public async Task<AppSplitTunnelingItem> GetAppAsync(SplitTunnelingApp app, SplitTunnelingMode splitTunnelingMode)
    {
        SplitTunnelingGroupType groupType = splitTunnelingMode is SplitTunnelingMode.Inverse
            ? SplitTunnelingGroupType.ProtectedApps
            : SplitTunnelingGroupType.ExcludedApps;

        string appName = app.AppFilePath.GetAppName();
        ImageSource? appIcon = await app.AppFilePath.GetAppIconAsync();

        return new AppSplitTunnelingItem(_localizer, groupType, app, appName, appIcon);
    }

    public IpAddressSplitTunnelingItem GetIpAddress(SplitTunnelingIpAddress ipAddress, SplitTunnelingMode splitTunnelingMode)
    {
        SplitTunnelingGroupType groupType = splitTunnelingMode is SplitTunnelingMode.Inverse
            ? SplitTunnelingGroupType.ProtectedIpAddresses
            : SplitTunnelingGroupType.ExcludedIpAddresses;

        return new IpAddressSplitTunnelingItem(_localizer, groupType, ipAddress);
    }
}