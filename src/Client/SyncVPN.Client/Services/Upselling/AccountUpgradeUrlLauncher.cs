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
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.Services.Upselling;

public class AccountUpgradeUrlLauncher : IAccountUpgradeUrlLauncher,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IUpsellUpgradeAttemptReporter _upsellUpgradeAttemptReporter;
    private readonly IUpsellSuccessReporter _upsellSuccessReporter;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IBackendModeProvider _backendModeProvider;

    private string? _currentAttemptUrl;
    private ModalSource? _currentAttemptModalSource;
    private string? _currentAttemptReference;

    public AccountUpgradeUrlLauncher(
        IUpsellUpgradeAttemptReporter upsellUpgradeAttemptReporter,
        IUpsellSuccessReporter upsellSuccessReporter,
        IUrlsBrowser urlsBrowser,
        IWebAuthenticator webAuthenticator,
        IBackendModeProvider backendModeProvider)
    {
        _upsellUpgradeAttemptReporter = upsellUpgradeAttemptReporter;
        _upsellSuccessReporter = upsellSuccessReporter;
        _urlsBrowser = urlsBrowser;
        _webAuthenticator = webAuthenticator;
        _backendModeProvider = backendModeProvider;
    }

    // A device-registered guest (see MainWindowViewNavigator.IsGuestAccessEnabled) has no Proton account
    // to fork a session for - _webAuthenticator.GetUpgradeAccountUrlAsync's auth-fork silently fails for
    // one and falls back to the bare Proton account URL, so "Upgrade" was opening account.protonvpn.com
    // instead of anything SyncVPN-branded. While the new backend is enabled, send everyone to the SyncVPN
    // pricing page instead of ever asking Proton for an upgrade URL.
    public async Task OpenAsync(ModalSource modalSource, string? reference = null)
    {
        string url = _backendModeProvider.IsNewBackendEnabled(BackendCapability.DeviceRegistration)
            ? _urlsBrowser.CreateAccount
            : await _webAuthenticator.GetUpgradeAccountUrlAsync(modalSource, reference);

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
