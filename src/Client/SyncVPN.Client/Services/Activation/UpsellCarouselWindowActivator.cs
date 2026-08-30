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

using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Activation.Bases;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Core.Services.Selection;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Dialogs.Upsell;
using SyncVPN.Logging.Contracts;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.Services.Activation;

public class UpsellCarouselWindowActivator : DialogActivatorBase<UpsellCarouselWindow>, IUpsellCarouselWindowActivator,
    IEventMessageReceiver<LoggedOutMessage>
{
    private readonly IUpsellDisplayReporter _upsellDisplayReporter;
    private readonly IUpsellCarouselViewNavigator _upsellCarouselViewNavigator;

    public override string WindowTitle => Localizer.Get("Upsell_Carousel_Title");

    public ModalSource ModalSource { get; private set; } = ModalSource.Undefined;

    public UpsellCarouselWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationService localizationService,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IMainWindowActivator mainWindowActivator,
        IUpsellDisplayReporter upsellDisplayReporter,
        IUpsellCarouselViewNavigator upsellCarouselViewNavigator)
        : base(logger,
               uiThreadDispatcher,
               themeSelector,
               settings,
               localizationService,
               localizer,
               iconSelector,
               mainWindowActivator)
    {
        _upsellDisplayReporter = upsellDisplayReporter;
        _upsellCarouselViewNavigator = upsellCarouselViewNavigator;
    }

    public Task<bool> ActivateAsync(UpsellFeatureType? upsellFeatureType)
    {
        Activate();

        SetCorrespondingModalSources(upsellFeatureType);

        _upsellDisplayReporter.Report(ModalSource);

        return _upsellCarouselViewNavigator.NavigateToFeatureViewAsync(upsellFeatureType);
    }

    private void SetCorrespondingModalSources(UpsellFeatureType? upsellFeatureType)
    {
        ModalSource = upsellFeatureType switch
        {
            UpsellFeatureType.WorldwideCoverage => ModalSource.Countries,
            UpsellFeatureType.Speed => ModalSource.VpnAccelerator,
            UpsellFeatureType.Streaming => ModalSource.Streaming,
            UpsellFeatureType.NetShield => ModalSource.NetShield,
            UpsellFeatureType.SecureCore => ModalSource.SecureCore,
            UpsellFeatureType.P2P => ModalSource.P2P,
            UpsellFeatureType.MultipleDevices => ModalSource.CarouselMultipleDevices,
            UpsellFeatureType.Tor => ModalSource.Tor,
            UpsellFeatureType.SplitTunneling => ModalSource.SplitTunneling,
            UpsellFeatureType.Profiles => ModalSource.Profiles,
            UpsellFeatureType.AdvancedSettings => ModalSource.CarouselCustomization,
            UpsellFeatureType.ModerateNat => ModalSource.ModerateNat,
            UpsellFeatureType.CustomDns => ModalSource.CustomDns,
            UpsellFeatureType.AllowLanConnections => ModalSource.AllowLanConnections,
            _ => ModalSource.Undefined
        };
    }

    public void Receive(LoggedOutMessage message)
    {
        Hide();
    }
}