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

using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.Common.Enums;
using SyncVPN.Client.Common.Models;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Profiles.Contracts;
using SyncVPN.Client.Logic.Profiles.Contracts.Models;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Contracts.Profiles;
using SyncVPN.Common.Core.Geographical;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;
using SyncVPN.Client.Contracts.Enums;

namespace SyncVPN.Client.Models.Connections.Profiles;

public partial class ProfileConnectionItem : ConnectionItemBase
{
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IProfilesManager _profilesManager;
    private readonly IProfileEditor _profileEditor;

    public IConnectionProfile Profile { get; }

    public override ConnectionGroupType GroupType => ConnectionGroupType.Profiles;

    public override string Header => Profile.Name;

    public override string Description => Localizer.GetConnectionProfileSubtitle(Profile);

    public override string? ToolTip => IsRestricted
        ? Localizer.Get("Connections_Profiles_NotAvailable_Tooltip")
        : null;

    public override string SecondaryCommandAutomationId => $"Actions_for_{AutomationName}";

    public override VpnTriggerDimension VpnTriggerDimension { get; } = VpnTriggerDimension.Profile;

    public string? ExitCountryCode => Profile.Location.GetCountryCode();

    public bool IsTor => Profile.Feature is TorFeatureIntent;

    public bool IsP2P => Profile.Feature is P2PFeatureIntent;

    public bool IsSecureCore => Profile.Feature is SecureCoreFeatureIntent;

    public FlagType FlagType => Profile.GetFlagType();

    public override object FirstSortProperty => Profile.CreationDateTimeUtc;

    public override object SecondSortProperty => Header;

    public ProfileConnectionItem(
        ILocalizationProvider localizer,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IProfilesManager profilesManager,
        IProfileEditor profileEditor,
        IConnectionProfile profile)
        : base(localizer,
               serversLoader,
               connectionManager,
               upsellCarouselWindowActivator,
               false)
    {
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _profilesManager = profilesManager;
        _profileEditor = profileEditor;

        Profile = profile;
    }

    public override IConnectionIntent GetConnectionIntent()
    {
        return Profile;
    }

    public override void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        base.InvalidateIsActiveConnection(currentConnectionDetails);

        DeleteProfileCommand.NotifyCanExecuteChanged();
    }

    protected override bool MatchesActiveConnection(ConnectionDetails? currentConnectionDetails)
    {
        return Profile.IsSameAs(currentConnectionDetails?.OriginalConnectionIntent);
    }

    [RelayCommand]
    private Task EditProfileAsync()
    {
        return _profileEditor.EditProfileAsync(Profile);
    }

    [RelayCommand]
    private Task DuplicateProfileAsync()
    {
        return _profileEditor.DuplicateProfileAsync(Profile);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteProfile))]
    private async Task DeleteProfileAsync()
    {
        ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(
            new MessageDialogParameters
            {
                Title = Localizer.Get("Connections_Profiles_Delete_Title"),
                Message = Localizer.GetFormat("Connections_Profiles_Delete_Message", Profile.Name),
                PrimaryButtonText = Localizer.Get("Connections_Profiles_Delete"),
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
            });

        if (result == ContentDialogResult.Primary)
        {
            _profilesManager.DeleteProfile(Profile.Id);
        }
    }

    private bool CanDeleteProfile()
    {
        return !IsActiveConnection;
    }

    public void InvalidateIsUnderMaintenance(IEnumerable<Server> servers, DeviceLocation? deviceLocation)
    {
        IsUnderMaintenance = GetConnectionIntent().AreAllServersUnderMaintenance(servers, deviceLocation);
    }
}