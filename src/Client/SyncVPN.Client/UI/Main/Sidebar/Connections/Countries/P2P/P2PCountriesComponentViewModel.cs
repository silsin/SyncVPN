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
using SyncVPN.Client.Logic.Servers.Contracts.Enums;
using SyncVPN.Client.Models.Connections;
using SyncVPN.Client.Services.Upselling;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.ViewModels;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.UI.Main.Sidebar.Connections.Countries.P2P;

public class P2PCountriesComponentViewModel : CountriesComponentViewModelBase
{
    public override CountriesConnectionType ConnectionType { get; } = CountriesConnectionType.P2P;

    public override string Header => Localizer.Get("Countries_P2P");

    public override string Description => Localizer.Get("Countries_P2P_Description");

    public override bool IsInfoBannerVisible => !IsUpsellBannerVisible
                                             && !Settings.IsP2PInfoBannerDismissed;

    public override int SortIndex { get; } = 2;

    protected override ModalSource UpsellModalSource => ModalSource.P2P;

    public P2PCountriesComponentViewModel(
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
            ServersLoader.GetCountriesByFeatures(ServerFeatures.P2P)
                         .Select(c => LocationItemFactory.GetP2PCountry(c));

        return genericCountries
            .Concat(countries);
    }

    protected override void DismissInfoBanner()
    {
        Settings.IsP2PInfoBannerDismissed = true;
    }

    protected override void OnSettingsChanged(string propertyName)
    {
        if (propertyName == nameof(ISettings.IsP2PInfoBannerDismissed))
        {
            OnPropertyChanged(nameof(IsInfoBannerVisible));
        }
    }
}