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

using System;
using Microsoft.UI.Xaml;
using SyncVPN.Client.Core.Bases;

namespace SyncVPN.Client.UI.Main.Settings.Pages.Connection;

public sealed partial class DnsFiltersPageView : IContextAware
{
    public DnsFiltersPageViewModel ViewModel { get; }

    public DnsFiltersPageView()
    {
        ViewModel = App.GetService<DnsFiltersPageViewModel>();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        ViewModel.ResetContentScrollRequested += OnResetContentScrollRequested;
    }

    public object GetContext() => ViewModel;

    private void OnLoaded(object sender, RoutedEventArgs e) => ViewModel.Activate();

    private void OnUnloaded(object sender, RoutedEventArgs e) => ViewModel.Deactivate();

    private void OnResetContentScrollRequested(object? sender, EventArgs e) => PageContentHost.ResetContentScroll();
}
