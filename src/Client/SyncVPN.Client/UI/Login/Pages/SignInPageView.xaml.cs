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
using Microsoft.UI.Xaml.Input;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Logic.Auth.Contracts.Enums;
using SyncVPN.Common.Core.Extensions;
using Windows.System;

namespace SyncVPN.Client.UI.Login.Pages;

public sealed partial class SignInPageView : IContextAware
{
    public SignInPageViewModel ViewModel { get; }

    public SignInPageView()
    {
        ViewModel = App.GetService<SignInPageViewModel>();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        UsernameTextBox.Loaded += OnUsernameTextBoxLoaded;
        PasswordBox.PasswordChanged += OnPasswordChanged;
        ViewModel.OnPasswordClearRequested += OnPasswordClearRequested;
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

    private void OnPasswordClearRequested(object? sender, EventArgs e)
    {
        PasswordBox.Password = string.Empty;
    }

    private void OnUsernameTextBoxLoaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel.AuthError == AuthError.GuestHoleFailed)
        {
            return;
        }

        UsernameTextBox.Focus(FocusState.Programmatic);
        UsernameTextBox.SelectAll();
    }

    private void OnFormBoxKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && SignInButton.Command.CanExecute(null))
        {
            SignInButton.Command.Execute(null);
        }
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.Password = PasswordBox.Password.ToSecureString();
    }
}