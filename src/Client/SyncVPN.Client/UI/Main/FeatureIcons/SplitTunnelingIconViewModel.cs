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
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Helpers;
using SyncVPN.Client.Core.Services.Selection;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Enums;

namespace SyncVPN.Client.UI.Main.FeatureIcons;

public class SplitTunnelingIconViewModel : FeatureIconViewModelBase
{
    protected override bool IsFeatureEnabled => Settings.IsSplitTunnelingEnabled;

    public SplitTunnelingIconViewModel(
        IConnectionManager connectionManager,
        ISettings settings,
        IApplicationThemeSelector themeSelector,
        IViewModelHelper viewModelHelper)
        : base(connectionManager, settings, themeSelector, viewModelHelper)
    { }

    protected override ImageSource GetImageSource()
    {
        return ResourceHelper.GetIllustration(
            IsFeatureEnabled
                ? (Settings.SplitTunnelingMode == SplitTunnelingMode.Standard
                    ? "SplitTunnelingStandardIllustrationSource"
                    : "SplitTunnelingInverseIllustrationSource")
                : "SplitTunnelingOffIllustrationSource",
            ThemeSelector.GetTheme());
    }

    protected override IEnumerable<string> GetSettingsChangedForIconUpdate()
    {
        yield return nameof(ISettings.IsSplitTunnelingEnabled);
        yield return nameof(ISettings.SplitTunnelingMode);
    }
}