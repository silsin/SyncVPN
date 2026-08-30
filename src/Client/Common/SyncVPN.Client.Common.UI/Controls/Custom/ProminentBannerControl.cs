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

using Microsoft.UI.Xaml;
using SyncVPN.Client.Common.Enums;

namespace SyncVPN.Client.Common.UI.Controls.Custom;

public class ProminentBannerControl : BannerControlBase
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public static readonly DependencyProperty BannerStyleProperty =
        DependencyProperty.Register(nameof(BannerStyle), typeof(ProminentBannerStyle), typeof(ProminentBannerControl), new PropertyMetadata(default));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public ProminentBannerStyle BannerStyle
    {
        get => (ProminentBannerStyle)GetValue(BannerStyleProperty);
        set => SetValue(BannerStyleProperty, value);
    }
}