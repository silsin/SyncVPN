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

using Microsoft.UI.Xaml;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Services.Navigation;

namespace SyncVPN.Client.UI.Dialogs.Upsell;

public sealed partial class UpsellCarouselShellView : IContextAware
{
    public UpsellCarouselShellViewModel ViewModel { get; }

    public UpsellCarouselViewNavigator Navigator { get; }

    public UpsellCarouselShellView()
    {
        ViewModel = App.GetService<UpsellCarouselShellViewModel>();
        Navigator = App.GetService<UpsellCarouselViewNavigator>();

        InitializeComponent();

        Navigator.Initialize(UpsellCarouselNavigationFrame);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Navigator.Load();
        ViewModel.Activate();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
        Navigator.Unload();

        Navigator.Reset();
    }
}