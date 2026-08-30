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

using SyncVPN.Client.Common.Enums;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Logic.Announcements.Contracts;
using SyncVPN.Client.Logic.Announcements.Contracts.Entities;
using SyncVPN.Client.Models.Announcements;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.UI.Main.Components.Banners;

public partial class ProminentBannerViewModel : BannerViewModelBase
{
    public string? Header => ActiveAnnouncement?.Panel?.Title;

    public string? Description => ActiveAnnouncement?.Panel?.Description;

    public ProminentBannerStyle BannerStyle => ActiveAnnouncement?.Panel is null ? ProminentBannerStyle.Regular : ActiveAnnouncement.Panel.Style;

    protected override AnnouncementType AnnouncementType { get; } = AnnouncementType.ProminentBanner;

    protected override ModalSource ModalSource { get; } = ModalSource.Account;

    public ProminentBannerViewModel(
        IAnnouncementActivator announcementActivator,
        IAnnouncementsProvider announcementsProvider,
        IUpsellDisplayReporter upsellDisplayReporter,
        IViewModelHelper viewModelHelper)
        : base(announcementActivator, announcementsProvider, upsellDisplayReporter, viewModelHelper)
    { }

    protected override void AfterAnnouncementChange()
    {
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(BannerStyle));
    }
}