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

using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Models.Connections;
using SyncVPN.Client.Services.FreeServers;
using SyncVPN.Client.Services.Upselling;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.UI.Main.Sidebar.Connections.Countries.All;

public class AllCountriesComponentViewModel : CountriesComponentViewModelBase
{
    private readonly IFreeServersCache _freeServersCache;
    private readonly IUserAuthenticator _userAuthenticator;

    public override CountriesConnectionType ConnectionType { get; } = CountriesConnectionType.All;

    public override string Header => Localizer.Get("Countries_All");

    public override int SortIndex { get; } = 0;

    public override string Description => string.Empty;

    public override bool IsInfoBannerVisible => false;

    public bool IsFreePlanBannerVisible => !Settings.VpnPlan.IsPaid;

    protected override ModalSource UpsellModalSource => ModalSource.Countries;

    public AllCountriesComponentViewModel(
        ISettings settings,
        IServersLoader serversLoader,
        ILocationItemFactory locationItemFactory,
        IFreeServersCache freeServersCache,
        IUserAuthenticator userAuthenticator,
        IViewModelHelper viewModelHelper,
        IAccountUpgradeUrlLauncher accountUpgradeUrlLauncher)
        : base(settings,
               serversLoader,
               locationItemFactory,
               viewModelHelper,
               accountUpgradeUrlLauncher)
    {
        _freeServersCache = freeServersCache;
        _userAuthenticator = userAuthenticator;
    }

    public override IEnumerable<ConnectionItemBase> GetItems()
    {
        // Anonymous/logged-out sessions only ever have the new backend's free-server catalog to show -
        // no legacy backend countries (requires login), no plan to gate Pro rows against - so only the
        // free subset is shown; a Pro row here would imply an unlockable server with nothing behind it.
        if (!_userAuthenticator.IsLoggedIn)
        {
            return _freeServersCache.GetServers()
                                     .Where(s => s.Free == 1)
                                     .Select(s => LocationItemFactory.GetSyncVpnServer(s));
        }

        IEnumerable<ConnectionItemBase> genericCountries = base.GetItems();

        IEnumerable<ConnectionItemBase> countries =
            ServersLoader.GetCountries()
                         .Select(c => LocationItemFactory.GetCountry(c));

        IEnumerable<ConnectionItemBase> syncVpnServers =
            _freeServersCache.GetServers()
                              .Select(s => LocationItemFactory.GetSyncVpnServer(s));

        return genericCountries
            .Concat(countries)
            .Concat(syncVpnServers);
    }

    protected override void DismissInfoBanner()
    { }
}