/*
 * Copyright (c) 2026 Proton AG
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
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Services.Activation;

namespace SyncVPN.Client.UI.Main.Home.Details.Flyouts;

public sealed partial class FreeServersFlyoutView : IContextAware
{
    public FreeServersFlyoutViewModel ViewModel { get; }

    public FreeServersFlyoutView()
    {
        ViewModel = App.GetService<FreeServersFlyoutViewModel>();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Activate();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
    }

    private void OnServerClicked(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not FreeServerItem item)
        {
            return;
        }

        ViewModel.ConnectCommand.Execute(item);

        // Close the enclosing Flyout - there's no direct reference to it from this nested UserControl,
        // so close whatever popup is currently open on the main window (the Flyout's own popup).
        Window? mainWindow = App.GetService<IMainWindowActivator>().Window;
        if (mainWindow is not null)
        {
            foreach (Popup popup in VisualTreeHelper.GetOpenPopups(mainWindow))
            {
                popup.IsOpen = false;
            }
        }
    }
}
