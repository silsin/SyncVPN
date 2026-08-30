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

using Microsoft.UI.Xaml.Media;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Core.Helpers;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.UI.Dialogs.Upsell.Bases;

namespace SyncVPN.Client.UI.Dialogs.Upsell.Features;

public class P2PUpsellFeaturePageViewModel : UpsellFeaturePageViewModelBase
{
    public override string Title => Localizer.Get("Upsell_Carousel_P2P");

    public override ImageSource SmallIllustrationSource { get; } = ResourceHelper.GetIllustration("P2PUpsellSmallIllustrationSource");

    public override ImageSource LargeIllustrationSource { get; } = ResourceHelper.GetIllustration("P2PUpsellLargeIllustrationSource");

    public P2PUpsellFeaturePageViewModel(
        IUpsellCarouselViewNavigator upsellCarouselViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(upsellCarouselViewNavigator,
               viewModelHelper,
               UpsellFeatureType.P2P)
    {
    }
}