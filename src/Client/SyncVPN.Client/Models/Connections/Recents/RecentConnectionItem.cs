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

using CommunityToolkit.Mvvm.Input;
using SyncVPN.Client.Common.Enums;
using SyncVPN.Client.Contracts.Enums;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Profiles.Contracts.Models;
using SyncVPN.Client.Logic.Recents.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Common.Core.Geographical;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.Models.Connections.Recents;

public partial class RecentConnectionItem : ConnectionItemBase
{
    private readonly ISettings _settings;
    private readonly IRecentConnectionsManager _recentConnectionsManager;

    public IRecentConnection RecentConnection { get; }

    public IConnectionProfile? Profile => RecentConnection.ConnectionIntent as IConnectionProfile;

    public override ConnectionGroupType GroupType => RecentConnection.IsPinned
        ? ConnectionGroupType.PinnedRecents
        : ConnectionGroupType.Recents;

    public override string Header => IsProfileIntent
        ? Profile!.Name
        : Localizer.GetConnectionIntentTitle(RecentConnection.ConnectionIntent);

    public override string Description => IsProfileIntent
        ? Localizer.GetConnectionProfileSubtitle(Profile)
        : Localizer.GetConnectionIntentSubtitle(RecentConnection.ConnectionIntent, false);

    public bool IsProfileIntent => Profile != null;

    public string? ExitCountry => RecentConnection.ConnectionIntent?.Location?.GetCountryCode();

    public string? EntryCountry => (RecentConnection.ConnectionIntent?.Feature as SecureCoreFeatureIntent)?.EntryCountryCode;

    public override object FirstSortProperty => RecentConnection.IsPinned;

    public override object SecondSortProperty => RecentConnection.PinTime ?? DateTime.MaxValue;

    public override VpnTriggerDimension VpnTriggerDimension { get; } = VpnTriggerDimension.Recent;

    public override string? ToolTip =>
        IsRestricted
            ? Localizer.Get("Home_Recents_Restricted")
            : IsUnderMaintenance
                ? Localizer.Get("Home_Recents_ServerUnderMaintenance")
                : null;

    public bool IsSecureCore => RecentConnection.ConnectionIntent?.Feature is SecureCoreFeatureIntent;

    public bool IsTor => RecentConnection.ConnectionIntent?.Feature is TorFeatureIntent;

    public bool IsP2P => RecentConnection.ConnectionIntent?.Feature is P2PFeatureIntent;

    public bool IsB2B => RecentConnection.ConnectionIntent?.Feature is B2BFeatureIntent;

    public FlagType FlagType => RecentConnection.ConnectionIntent.GetFlagType();

    public override string SecondaryCommandAutomationId => $"Actions_for_{AutomationName}";

    public RecentConnectionItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        ISettings settings,
        IRecentConnectionsManager recentConnectionsManager,
        IRecentConnection recentConnection)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator,
               false)
    {
        _settings = settings;
        _recentConnectionsManager = recentConnectionsManager;

        RecentConnection = recentConnection;
    }

    public override IConnectionIntent GetConnectionIntent()
    {
        return RecentConnection.ConnectionIntent;
    }

    public override void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        base.InvalidateIsActiveConnection(currentConnectionDetails);

        RemoveCommand.NotifyCanExecuteChanged();
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return currentConnectionDetails != null
            && RecentConnection.ConnectionIntent.IsSameAs(currentConnectionDetails.OriginalConnectionIntent)
            && RecentConnection.ConnectionIntent.IsSupported(currentConnectionDetails.Server, _settings.DeviceLocation);
    }

    [RelayCommand(CanExecute = nameof(CanPin))]
    private void Pin()
    {
        _recentConnectionsManager.Pin(RecentConnection);
    }

    private bool CanPin()
    {
        return !RecentConnection.IsPinned;
    }

    [RelayCommand(CanExecute = nameof(CanUnpin))]
    private void Unpin()
    {
        _recentConnectionsManager.Unpin(RecentConnection);
    }

    private bool CanUnpin()
    {
        return RecentConnection.IsPinned;
    }

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void Remove()
    {
        _recentConnectionsManager.Remove(RecentConnection);
    }

    private bool CanRemove()
    {
        return !IsActiveConnection;
    }

    public void InvalidateIsUnderMaintenance(IEnumerable<Server> servers, DeviceLocation? deviceLocation)
    {
        IsUnderMaintenance = GetConnectionIntent().AreAllServersUnderMaintenance(servers, deviceLocation);
    }
}