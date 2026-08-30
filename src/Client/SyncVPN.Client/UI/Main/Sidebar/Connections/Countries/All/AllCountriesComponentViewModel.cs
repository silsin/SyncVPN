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
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Models.Connections;
using SyncVPN.Client.Services.Upselling;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.UI.Main.Sidebar.Connections.Countries.All;

public class AllCountriesComponentViewModel : CountriesComponentViewModelBase
{
    public override CountriesConnectionType ConnectionType { get; } = CountriesConnectionType.All;

    public override string Header => Localizer.Get("Countries_All");

    public override int SortIndex { get; } = 0;

    public override string Description => string.Empty;

    public override bool IsInfoBannerVisible => false;

    protected override ModalSource UpsellModalSource => ModalSource.Countries;

    public AllCountriesComponentViewModel(
        ISettings settings,
        IServersLoader serversLoader,
        ILocationItemFactory locationItemFactory,
        IViewModelHelper viewModelHelper,
        IAccountUpgradeUrlLauncher accountUpgradeUrlLauncher)
        : base(settings,
               serversLoader,
               locationItemFactory,
               viewModelHelper,
               accountUpgradeUrlLauncher)
    { }

    public override IEnumerable<ConnectionItemBase> GetItems()
    {
        IEnumerable<ConnectionItemBase> genericCountries = base.GetItems();

        IEnumerable<ConnectionItemBase> countries =
            ServersLoader.GetCountries()
                         .Select(c => LocationItemFactory.GetCountry(c));

        return genericCountries
            .Concat(countries);
    }

    protected override void DismissInfoBanner()
    { }
}