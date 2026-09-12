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

namespace SyncVPN.Client.UI.Main.Home;

public sealed partial class HomeComponentView : IContextAware
{
    public static readonly DependencyProperty SidebarWidthProperty = DependencyProperty.Register(
        nameof(SidebarWidth),
        typeof(double),
        typeof(HomeComponentView),
        new PropertyMetadata(default));

    public static readonly DependencyProperty MapTopOffsetProperty = DependencyProperty.Register(
        nameof(MapTopOffset),
        typeof(double),
        typeof(HomeComponentView),
        new PropertyMetadata(default));

    public static readonly DependencyProperty MapBottomOffsetProperty = DependencyProperty.Register(
        nameof(MapBottomOffset),
        typeof(double),
        typeof(HomeComponentView),
        new PropertyMetadata(default));

    public static readonly DependencyProperty IsHomeDisplayedProperty = DependencyProperty.Register(
        nameof(IsHomeDisplayed),
        typeof(bool),
        typeof(HomeComponentView),
        new PropertyMetadata(default));

    public HomeComponentViewModel ViewModel { get; }

    public double SidebarWidth
    {
        get => (double)GetValue(SidebarWidthProperty);
        set => SetValue(SidebarWidthProperty, value);
    }

    public double MapTopOffset
    {
        get => (double)GetValue(MapTopOffsetProperty);
        set => SetValue(MapTopOffsetProperty, value);
    }

    public double MapBottomOffset
    {
        get => (double)GetValue(MapBottomOffsetProperty);
        set => SetValue(MapBottomOffsetProperty, value);
    }

    public bool IsHomeDisplayed
    {
        get => (bool)GetValue(IsHomeDisplayedProperty);
        set => SetValue(IsHomeDisplayedProperty, value);
    }

    public HomeComponentView()
    {
        ViewModel = App.GetService<HomeComponentViewModel>();

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
        ConnectionCardComponent.SizeChanged += InvalidateMapOffsets;
        BannersContainer.SizeChanged += InvalidateMapOffsets;
    }

    private void InvalidateMapOffsets(object sender, SizeChangedEventArgs e)
    {
        MapBottomOffset = BannersContainer.ActualHeight + ConnectionCardComponent.ActualHeight;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
        ConnectionCardComponent.SizeChanged -= InvalidateMapOffsets;
        BannersContainer.SizeChanged -= InvalidateMapOffsets;
    }
}