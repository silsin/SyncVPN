/*
 * Copyright (c) 2026 Proton AG
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
using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts;
using SyncVPN.Api.V2.Contracts.Devices;
using SyncVPN.Client.Common.Observers;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.IssueReporting.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.ApiLogs;

namespace SyncVPN.Client.Logic.Auth;

// Registers this installation with the new SyncVPN backend (POST /devices/register) once per app start.
// Prerequisite for every later migration phase, so it runs independently of and ahead of any other
// BackendCapability - but stays fully inert (no network call at all) until BackendCapability.DeviceRegistration
// is enabled, per the migration plan's "old backend stays the default until proven" requirement.
public class DeviceRegistrationObserver : ObserverBase
{
    private readonly IBackendModeProvider _backendModeProvider;
    private readonly ISyncVpnApiClient _apiClient;
    private readonly ISettings _settings;

    public DeviceRegistrationObserver(
        ILogger logger,
        IIssueReporter issueReporter,
        IBackendModeProvider backendModeProvider,
        ISyncVpnApiClient apiClient,
        ISettings settings)
        : base(logger, issueReporter)
    {
        _backendModeProvider = backendModeProvider;
        _apiClient = apiClient;
        _settings = settings;

        TriggerAction.Run();
    }

    protected override async Task OnTriggerAsync()
    {
        if (!_backendModeProvider.IsNewBackendEnabled(BackendCapability.DeviceRegistration))
        {
            return;
        }

        string deviceId = _settings.SyncVpnDeviceId ?? Guid.NewGuid().ToString();
        RegisterDeviceRequest request = new() { DeviceId = deviceId, Language = _settings.Language };

        Logger.Info<ApiLog>("Registering device with the new SyncVPN backend.");

        ApiResponseResult<RegisterDeviceResponse> response = await _apiClient.RegisterDeviceAsync(request);
        if (response.Success)
        {
            _settings.SyncVpnDeviceId = deviceId;
        }
        else
        {
            Logger.Error<ApiErrorLog>($"Failed to register device with the new SyncVPN backend: {response.Error}");
        }
    }
}
