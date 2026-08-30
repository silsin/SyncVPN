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
using SyncVPN.Client.UI.Main.Sidebar.Connections.Countries.All;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Countries.P2P;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Countries.SecureCore;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Countries.Tor;

namespace SyncVPN.Client.Selectors;

public class CountriesComponentItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? AllCountriesItemTemplate { get; set; }

    public DataTemplate? SecureCoreCountriesItemTemplate { get; set; }

    public DataTemplate? P2PCountriesItemTemplate { get; set; }

    public DataTemplate? TorCountriesItemTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        DataTemplate? template = item switch
        {
            AllCountriesComponentViewModel => AllCountriesItemTemplate,
            SecureCoreCountriesComponentViewModel => SecureCoreCountriesItemTemplate,
            P2PCountriesComponentViewModel => P2PCountriesItemTemplate,
            TorCountriesComponentViewModel => TorCountriesItemTemplate,
            _ => throw new NotSupportedException($"Countries component '{item}' is not recognized"),
        };

        return template ?? throw new NotSupportedException("Countries component item data template is undefined");
    }
}