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

using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Extensions;
using SyncVPN.Client.Services.Activation;

namespace SyncVPN.Client.UI.Dialogs.Tray;

public sealed partial class TrayAppWindow : IFocusAware
{
    public TrayAppWindowActivator WindowActivator { get; }

    public TrayAppWindow()
    {
        WindowActivator = App.GetService<TrayAppWindowActivator>();
        this.TrySetIsShownInSwitchers(false);
        InitializeComponent();
        WindowActivator.Initialize(this);
    }

    public void OnFocusChanged()
    {
        
    }

    public bool IsFocused()
    {
        return WindowActivator.IsWindowFocused;
    }
}