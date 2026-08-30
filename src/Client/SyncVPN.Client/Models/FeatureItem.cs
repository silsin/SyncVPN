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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SyncVPN.Client.Core.Bases.Models;
using SyncVPN.Client.Core.Helpers;
using SyncVPN.Client.Core.Services.Selection;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;

namespace SyncVPN.Client.Models;

public class FeatureItem : ModelBase
{    
    public Feature Feature { get; }

    public string Header => Localizer.GetFeatureName(Feature);

    public string Description => Localizer.GetFeatureDescription(Feature);

    public ImageSource IllustrationSource { get; }

    public FeatureItem(
        ILocalizationProvider localizer,
        IApplicationThemeSelector themeSelector,
        Feature feature)
        : base(localizer)
    {
        Feature = feature;

        ElementTheme theme = themeSelector.GetTheme();

        IllustrationSource = feature switch
        {
            Feature.B2B => ResourceHelper.GetIllustration("GatewaysIllustrationSource", theme),
            Feature.P2P => ResourceHelper.GetIllustration("P2PUpsellSmallIllustrationSource", theme),
            Feature.SecureCore => ResourceHelper.GetIllustration("SecureCoreUpsellSmallIllustrationSource", theme),
            _ => ResourceHelper.GetIllustration("WrongCountryIllustrationSource", theme),
        };
    }
}