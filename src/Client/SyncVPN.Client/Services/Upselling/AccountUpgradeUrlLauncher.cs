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

using SyncVPN.Api.BackendSelection;
using SyncVPN.Client.Contracts.Services.Browsing;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.Services.Upselling;

public class AccountUpgradeUrlLauncher : IAccountUpgradeUrlLauncher,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private const string InAppStoreAttemptUrl = "app:store";

    private readonly IUpsellUpgradeAttemptReporter _upsellUpgradeAttemptReporter;
    private readonly IUpsellSuccessReporter _upsellSuccessReporter;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IBackendModeProvider _backendModeProvider;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly IMainWindowActivator _mainWindowActivator;

    private string? _currentAttemptUrl;
    private ModalSource? _currentAttemptModalSource;
    private string? _currentAttemptReference;

    public AccountUpgradeUrlLauncher(
        IUpsellUpgradeAttemptReporter upsellUpgradeAttemptReporter,
        IUpsellSuccessReporter upsellSuccessReporter,
        IUrlsBrowser urlsBrowser,
        IWebAuthenticator webAuthenticator,
        IBackendModeProvider backendModeProvider,
        IMainViewNavigator mainViewNavigator,
        IMainWindowActivator mainWindowActivator)
    {
        _upsellUpgradeAttemptReporter = upsellUpgradeAttemptReporter;
        _upsellSuccessReporter = upsellSuccessReporter;
        _urlsBrowser = urlsBrowser;
        _webAuthenticator = webAuthenticator;
        _backendModeProvider = backendModeProvider;
        _mainViewNavigator = mainViewNavigator;
        _mainWindowActivator = mainWindowActivator;
    }

    // Every "Upgrade"/"Upgrade to Premium" entry point in the app (country flags, tray, feature upsell
    // dialogs, etc.) funnels through here. This used to always open an external browser tab - a device
    // registered guest (see MainWindowViewNavigator.IsGuestAccessEnabled) has no legacy account to fork a
    // session for, so _webAuthenticator.GetUpgradeAccountUrlAsync's auth-fork silently failed for one and
    // fell back to the bare legacy account URL, meaning "Upgrade" opened the legacy account domain instead
    // of anything SyncVPN-branded - and even when that URL was right, a browser tab opening behind the
    // main window looked exactly like nothing happened. Now that the in-app Store page exists (with real
    // checkout-link purchase flow), send everyone there directly instead while the new backend is enabled.
    public async Task OpenAsync(ModalSource modalSource, string? reference = null)
    {
        if (_backendModeProvider.IsNewBackendEnabled(BackendCapability.DeviceRegistration))
        {
            try
            {
                _upsellUpgradeAttemptReporter.Report(modalSource, reference);

                _mainWindowActivator.Activate();
                await _mainViewNavigator.NavigateToStoreViewAsync();
            }
            finally
            {
                SetAttempt(InAppStoreAttemptUrl, modalSource, reference);
            }

            return;
        }

        string url = await _webAuthenticator.GetUpgradeAccountUrlAsync(modalSource, reference);

        Open(url, modalSource, reference);
    }

    public void Open(string url, ModalSource modalSource, string? reference = null)
    {
        try
        {
            _upsellUpgradeAttemptReporter.Report(modalSource, reference);

            _urlsBrowser.BrowseTo(url);
        }
        finally
        {
            SetAttempt(url, modalSource, reference);
        }
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        try
        {
            if (_currentAttemptModalSource.HasValue && message.HasChanged() && !message.IsDowngrade())
            {
                _upsellSuccessReporter.Report(
                    _currentAttemptUrl ?? string.Empty, 
                    _currentAttemptModalSource.Value, 
                    message.OldPlan, 
                    message.NewPlan, 
                    _currentAttemptReference);
            }
        }
        finally
        {
            ResetAttempt();
        }
    }

    private void SetAttempt(string url, ModalSource modalSource, string? reference)
    {
        _currentAttemptUrl = url;
        _currentAttemptModalSource = modalSource;
        _currentAttemptReference = reference;
    }

    private void ResetAttempt()
    {
        _currentAttemptUrl = null;
        _currentAttemptModalSource = null;
        _currentAttemptReference = null;
    }
}
