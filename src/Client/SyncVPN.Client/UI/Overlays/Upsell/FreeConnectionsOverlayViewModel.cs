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

using CommunityToolkit.Mvvm.Input;
using SyncVPN.Client.Common.Collections;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Searches;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Services.Upselling;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.UI.Overlays.Upsell;

public partial class FreeConnectionsOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>,
    IEventMessageReceiver<ServerListChangedMessage>
{
    private readonly IServersLoader _serversLoader;
    private readonly IAccountUpgradeUrlLauncher _accountUpgradeUrlLauncher;

    public SmartObservableCollection<LocalizedFreeCountry> FreeCountries { get; } = [];

    public string UpsellTagline
        => Localizer.GetFormat("Upsell_Carousel_WorldwideCoverage",
                Localizer.GetPluralFormat("Upsell_Carousel_WorldwideCoverage_Servers", _serversLoader.GetServerCount()),
                Localizer.GetPluralFormat("Upsell_Carousel_WorldwideCoverage_Countries", _serversLoader.GetCountryCount()));

    public long FreeCountriesCount => FreeCountries.Count;

    public FreeConnectionsOverlayViewModel(
        IMainWindowOverlayActivator overlayActivator,
        IServersLoader serversLoader,
        IAccountUpgradeUrlLauncher accountUpgradeUrlLauncher,
        IViewModelHelper viewModelHelper)
        : base(overlayActivator, viewModelHelper)
    {
        _serversLoader = serversLoader;
        _accountUpgradeUrlLauncher = accountUpgradeUrlLauncher;
        InvalidateFreeCountries();
    }

    public void Receive(ServerListChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateFreeCountries);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateFreeCountries();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        InvalidateFreeCountries();
    }

    private void InvalidateFreeCountries()
    {
        List<LocalizedFreeCountry> countries = _serversLoader
            .GetFreeCountries()
            .Select(c => new LocalizedFreeCountry()
            {
                Country = c,
                LocalizedName = Localizer.GetCountryName(c.Code)
            })
            .OrderBy(c => c.LocalizedName).
            ToList();

        FreeCountries.Reset(countries);

        OnPropertyChanged(nameof(FreeCountriesCount));
        OnPropertyChanged(nameof(UpsellTagline));
    }

    [RelayCommand]
    private async Task UpgradePlanAsync()
    {
        await _accountUpgradeUrlLauncher.OpenAsync(ModalSource.Countries);
    }
}