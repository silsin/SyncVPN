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

using SyncVPN.Client.Common.Messages;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;

namespace SyncVPN.Client.UI;

public class MainWindowShellViewModel : ShellViewModelBase<IMainWindowActivator, IMainWindowViewNavigator>
{
    private readonly IEventMessageSender _eventMessageSender;

    public MainWindowShellViewModel(
        IMainWindowActivator windowActivator,
        IMainWindowViewNavigator childViewNavigator,
        IEventMessageSender eventMessageSender,
        IViewModelHelper viewModelHelper)
        : base(windowActivator, childViewNavigator, viewModelHelper)
    {
        _eventMessageSender = eventMessageSender;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        _eventMessageSender.Send<ApplicationStartedMessage>();
    }
}