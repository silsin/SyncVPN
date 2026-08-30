/*
 * Copyright (c) 2024 Proton AG
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

using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.Common.UI.Assets.Icons.Base;
using SyncVPN.Client.Common.UI.Assets.Icons.PathIcons;
using SyncVPN.Client.Contracts.Profiles;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Profiles.Contracts;
using SyncVPN.Client.Logic.Profiles.Contracts.Messages;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Models.Connections;
using SyncVPN.Client.Models.Connections.Profiles;
using SyncVPN.Client.Services.Upselling;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;
using SyncVPN.Common.Core.Geographical;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.UI.Main.Sidebar.Connections.Profiles;

public partial class ProfilesPageViewModel : ConnectionPageViewModelBase,
    IEventMessageReceiver<ProfilesChangedMessage>
{
    private readonly IConnectionItemFactory _connectionItemFactory;
    private readonly IProfilesManager _profilesManager;
    private readonly IProfileEditor _profileEditor;
    private readonly IAccountUpgradeUrlLauncher _accountUpgradeUrlLauncher;

    public override string Header => Localizer.Get("Profiles_Page_Title");
    public override IconElement Icon => new WindowTerminal() { Size = PathIconSize.Pixels16 };
    public override int SortIndex { get; } = 3;
    public bool IsUpsellBannerVisible => !Settings.VpnPlan.IsPaid;
    public override bool IsAvailable => ParentViewNavigator.CanNavigateToProfilesView();

    public event EventHandler<ConnectionItemBase>? ScrollToItemRequested;

    public ProfilesPageViewModel(
        IConnectionsViewNavigator parentViewNavigator,
        ISettings settings,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IConnectionGroupFactory connectionGroupFactory,
        IConnectionItemFactory connectionItemFactory,
        IProfilesManager profilesManager,
        IProfileEditor profileEditor,
        IViewModelHelper viewModelHelper,
        IAccountUpgradeUrlLauncher accountUpgradeUrlLauncher)
        : base(parentViewNavigator,
               settings,
               serversLoader,
               connectionManager,
               connectionGroupFactory,
               viewModelHelper)
    {
        _connectionItemFactory = connectionItemFactory;
        _profilesManager = profilesManager;
        _profileEditor = profileEditor;
        _accountUpgradeUrlLauncher = accountUpgradeUrlLauncher;
    }

    public void Receive(ProfilesChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            FetchItems();

            switch (message.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    ProfileConnectionItem? profile = Items.OfType<ProfileConnectionItem>().FirstOrDefault(p => p.Profile.Id == message.ChangedProfileId);
                    if (profile != null)
                    {
                        ScrollToItemRequested?.Invoke(this, profile);
                    }
                    break;
                default:
                    break;
            }
        });
    }

    protected override IEnumerable<ConnectionItemBase> GetItems()
    {
        return _profilesManager.GetAll()
                               .Select(_connectionItemFactory.GetProfile);
    }

    protected override void InvalidateMaintenanceStates()
    {
        if (IsActive)
        {
            IEnumerable<Server> servers = ServersLoader.GetServers();
            DeviceLocation? deviceLocation = Settings.DeviceLocation;

            foreach (ProfileConnectionItem item in Items.OfType<ProfileConnectionItem>())
            {
                item.InvalidateIsUnderMaintenance(servers, deviceLocation);
            }
        }
    }

    protected override void OnVpnPlanChanged(VpnPlan oldPlan, VpnPlan newPlan)
    {
        base.OnVpnPlanChanged(oldPlan, newPlan);

        OnPropertyChanged(nameof(IsUpsellBannerVisible));
    }

    protected override void OnLoggedIn()
    {
        base.OnLoggedIn();

        OnPropertyChanged(nameof(IsUpsellBannerVisible));
    }

    [RelayCommand]
    private Task CreateProfileAsync()
    {
        return _profileEditor.CreateProfileAsync();
    }

    [RelayCommand]
    private async Task UpgradeAsync()
    {
        await _accountUpgradeUrlLauncher.OpenAsync(ModalSource.Profiles);
    }
}