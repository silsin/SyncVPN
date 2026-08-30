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
using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.UI.Main.Features.KillSwitch;
using SyncVPN.Client.UI.Main.Features.NetShield;
using SyncVPN.Client.UI.Main.Features.PortForwarding;
using SyncVPN.Client.UI.Main.Features.SplitTunneling;
using SyncVPN.Client.UI.Main.Settings;

namespace SyncVPN.Client.Selectors;

public class SideWidgetItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? DefaultWidgetTemplate { get; set; }

    public DataTemplate? KillSwitchWidgetTemplate { get; set; }

    public DataTemplate? NetShieldWidgetTemplate { get; set; }

    public DataTemplate? PortForwardingWidgetTemplate { get; set; }

    public DataTemplate? SplitTunnelingWidgetTemplate { get; set; }

    public DataTemplate? SettingsWidgetTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        DataTemplate? template = item switch
        {
            KillSwitchWidgetViewModel => KillSwitchWidgetTemplate,
            NetShieldWidgetViewModel => NetShieldWidgetTemplate,
            PortForwardingWidgetViewModel => PortForwardingWidgetTemplate,
            SplitTunnelingWidgetViewModel => SplitTunnelingWidgetTemplate,
            SettingsWidgetViewModel => SettingsWidgetTemplate,
            _ => DefaultWidgetTemplate
        };

        return template ?? throw new InvalidOperationException("Side widget item template is undefined");
    }
}