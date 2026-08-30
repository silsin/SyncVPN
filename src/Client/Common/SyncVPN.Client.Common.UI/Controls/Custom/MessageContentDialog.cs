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
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.Common.UI.Automation;

namespace SyncVPN.Client.Common.UI.Controls.Custom;

public class MessageContentDialog : ContentDialog
{
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(MessageContentDialog), new PropertyMetadata(default));

    public static readonly DependencyProperty IsVerticalLayoutProperty =
        DependencyProperty.Register(nameof(IsVerticalLayout), typeof(bool), typeof(MessageContentDialog), new PropertyMetadata(default));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool IsVerticalLayout
    {
        get => (bool)GetValue(IsVerticalLayoutProperty);
        set => SetValue(IsVerticalLayoutProperty, value);
    }

    public MessageContentDialog()
    {
        DefaultStyleKey = typeof(MessageContentDialog);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new MessageContentDialogAutomationPeer(this);
    }
}