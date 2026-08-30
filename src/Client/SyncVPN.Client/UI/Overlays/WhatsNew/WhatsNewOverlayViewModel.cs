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
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;

namespace SyncVPN.Client.UI.Overlays.WhatsNew;

public partial class WhatsNewOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly ISettings _settings;

    public bool IsSubscriptionBadgeVisible => !_settings.VpnPlan.IsPaid;

    public bool IsB2BSectionVisible => _settings.VpnPlan.IsB2B;

    public WhatsNewOverlayViewModel(
        ISettings settings,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IViewModelHelper viewModelHelper)
        : base(mainWindowOverlayActivator, viewModelHelper)
    {
        _settings = settings;
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            OnPropertyChanged(nameof(IsSubscriptionBadgeVisible));
            OnPropertyChanged(nameof(IsB2BSectionVisible));
        });
    }
}