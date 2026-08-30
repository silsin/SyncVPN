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

using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Services.Mapping;
using SyncVPN.Client.Services.Mapping.Bases;
using SyncVPN.Client.UI.Login.Overlays;
using SyncVPN.Client.UI.Overlays.Upsell;
using SyncVPN.Client.UI.Overlays.HumanVerification;
using SyncVPN.Client.UI.Overlays.Information;
using SyncVPN.Client.UI.Overlays.Information.Notification;
using SyncVPN.Client.UI.Overlays.Welcome;
using SyncVPN.Client.UI.Overlays.WhatsNew;
using SyncVPN.Client.UI.Overlays.Selection;

namespace SyncVPN.Client.Services.Mapping;

public class OverlayViewMapper : ViewMapperBase<OverlayViewModelBase, ContentDialog>, IOverlayViewMapper
{
    protected override void ConfigureMappings()
    {
        ConfigureMapping<HumanVerificationOverlayViewModel, HumanVerificationOverlayView>();

        ConfigureMapping<P2POverlayViewModel, P2POverlayView>();
        ConfigureMapping<SecureCoreOverlayViewModel, SecureCoreOverlayView>();
        ConfigureMapping<TorOverlayViewModel, TorOverlayView>();
        ConfigureMapping<SmartRoutingOverlayViewModel, SmartRoutingOverlayView>();
        ConfigureMapping<ProfileOverlayViewModel, ProfileOverlayView>();
        ConfigureMapping<ServerLoadOverlayViewModel, ServerLoadOverlayView>();
        ConfigureMapping<SsoLoginOverlayViewModel, SsoLoginOverlayView>();
        ConfigureMapping<OutdatedClientOverlayViewModel, OutdatedClientOverlayView>();

        ConfigureMapping<WelcomeOverlayViewModel, WelcomeOverlayView>();
        ConfigureMapping<WelcomeToVpnPlusOverlayViewModel, WelcomeToVpnPlusOverlayView>();
        ConfigureMapping<WelcomeToVpnUnlimitedOverlayViewModel, WelcomeToVpnUnlimitedOverlayView>();
        ConfigureMapping<WelcomeToVpnB2BOverlayViewModel, WelcomeToVpnB2BOverlayView>();

        ConfigureMapping<FreeConnectionsOverlayViewModel, FreeConnectionsOverlayView>();
        ConfigureMapping<WhatsNewOverlayViewModel, WhatsNewOverlayView>();

        ConfigureMapping<IpSelectorOverlayViewModel, IpSelectorOverlayView>();
        ConfigureMapping<AppSelectorOverlayViewModel, AppSelectorOverlayView>();
    }
}