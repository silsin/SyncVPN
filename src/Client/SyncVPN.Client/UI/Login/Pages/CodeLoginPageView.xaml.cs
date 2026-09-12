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
using Microsoft.UI.Xaml.Input;
using SyncVPN.Client.Core.Bases;
using Windows.System;

namespace SyncVPN.Client.UI.Login.Pages;

public sealed partial class CodeLoginPageView : IContextAware
{
    public CodeLoginPageViewModel ViewModel { get; }

    public CodeLoginPageView()
    {
        ViewModel = App.GetService<CodeLoginPageViewModel>();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        CodeTextBox.Loaded += OnCodeTextBoxLoaded;
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

    private void OnCodeTextBoxLoaded(object sender, RoutedEventArgs e)
    {
        CodeTextBox.Focus(FocusState.Programmatic);
    }

    private void OnCodeBoxKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.SignInWithCodeCommand.CanExecute(null))
        {
            ViewModel.SignInWithCodeCommand.Execute(null);
        }
    }
}
