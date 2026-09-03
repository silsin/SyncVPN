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

using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace SyncVPN.Client.Common.UI.Converters;

/// <summary>
/// Returns a bright green brush when true, or a dimmed near-white brush when false.
/// Used for elements (e.g. sidebar nav items) whose foreground should switch to the
/// brand green while selected/active.
///
/// This intentionally does NOT look up "VpnGreenColorBrush"/"TextWeakColorBrush" via
/// Application.Current.Resources: this app applies its dark theme per-element
/// (RequestedTheme on the window's root content), not via Application.RequestedTheme,
/// so an app-level resource lookup resolves the Light theme dictionary instead and
/// returns near-black text. The colors below are the app's dark-theme VpnGreenColor
/// (#22C55E) and TextWeakColor (#F5F7FA at 70% opacity) - see Colors.xaml.
/// </summary>
public class BooleanToVpnGreenForegroundConverter : IValueConverter
{
    private static readonly SolidColorBrush SelectedBrush = new(Color.FromArgb(0xFF, 0x22, 0xC5, 0x5E));
    private static readonly SolidColorBrush UnselectedBrush = new(Color.FromArgb(0xB3, 0xF5, 0xF7, 0xFA));

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isSelected = value is bool b && b;

        return isSelected ? SelectedBrush : UnselectedBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
