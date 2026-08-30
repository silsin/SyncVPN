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

using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Core.Messages;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Activation.Bases;
using SyncVPN.Client.Core.Services.Selection;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Dialogs.Upsell;
using SyncVPN.Logging.Contracts;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.Services.Activation;

public class P2PDetectionWindowActivator : DialogActivatorBase<P2PDetectionWindow>, IP2PDetectionWindowActivator,
    IEventMessageReceiver<LoggedOutMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IUpsellDisplayReporter _upsellDisplayReporter;

    public override string WindowTitle => Localizer.Get("Dialogs_P2PDetection_WindowTitle");

    public P2PDetectionWindowActivator(
        ILogger logger,
        IEventMessageSender eventMessageSender,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationService localizationService,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IMainWindowActivator mainWindowActivator,
        IUpsellDisplayReporter upsellDisplayReporter)
        : base(logger,
               uiThreadDispatcher,
               themeSelector,
               settings,
               localizationService,
               localizer,
               iconSelector,
               mainWindowActivator)
    {
        _eventMessageSender = eventMessageSender;
        _upsellDisplayReporter = upsellDisplayReporter;
    }

    protected override void OnWindowOpened()
    {
        base.OnWindowOpened();

        _upsellDisplayReporter.Report(ModalSource.P2PActivity);
    }

    protected override void OnWindowHidden()
    {
        base.OnWindowHidden();

        _eventMessageSender.Send<P2PWarningWindowClosedMessage>();
    }

    public void Receive(LoggedOutMessage message)
    {
        Hide();
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        if (message.IsUpgrade())
        {
            Hide();
        }
    }
}