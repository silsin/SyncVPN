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

using System.Globalization;
using System.Runtime.InteropServices;
using SyncVPN.Api.BackendSelection;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts;
using SyncVPN.Api.V2.Contracts.Devices;
using SyncVPN.Client.Common.Observers;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Configurations.Contracts;
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
    private readonly IConfiguration _config;
    private readonly IDeviceIdentifierProvider _deviceIdentifierProvider;

    public DeviceRegistrationObserver(
        ILogger logger,
        IIssueReporter issueReporter,
        IBackendModeProvider backendModeProvider,
        ISyncVpnApiClient apiClient,
        ISettings settings,
        IConfiguration config,
        IDeviceIdentifierProvider deviceIdentifierProvider)
        : base(logger, issueReporter)
    {
        _backendModeProvider = backendModeProvider;
        _apiClient = apiClient;
        _settings = settings;
        _config = config;
        _deviceIdentifierProvider = deviceIdentifierProvider;

        TriggerAction.Run();
    }

    protected override async Task OnTriggerAsync()
    {
        if (!_backendModeProvider.IsNewBackendEnabled(BackendCapability.DeviceRegistration))
        {
            return;
        }

        // Falls back to a stable, hardware-derived id (not a random one) so a device that lost its
        // settings (e.g. this app was uninstalled and reinstalled) re-registers under the same id
        // instead of the backend seeing it as a brand new device every time.
        string deviceId = _settings.SyncVpnDeviceId ?? _deviceIdentifierProvider.GetStableDeviceId();

        // No FCM/APNs or OneSignal SDK on Windows - these are locally-generated, persisted GUIDs, not
        // real push tokens (see IGlobalSettings.SyncVpnPushToken).
        string pushToken = _settings.SyncVpnPushToken ?? Guid.NewGuid().ToString();
        string oneSignalSubscriptionId = _settings.SyncVpnOneSignalSubscriptionId ?? Guid.NewGuid().ToString();

        RegisterDeviceRequest request = new()
        {
            DeviceId = deviceId,
            Language = _settings.Language,
            Name = Environment.MachineName,
            AppVersion = _config.ClientVersion,
            OsVersion = Environment.OSVersion.Version.ToString(),
            Architecture = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(),
            Locale = CultureInfo.CurrentUICulture.Name,
            Timezone = GetIanaTimezoneId(),
            PushToken = pushToken,
            OneSignalSubscriptionId = oneSignalSubscriptionId,
        };

        Logger.Info<ApiLog>("Registering device with the new SyncVPN backend.");

        ApiResponseResult<RegisterDeviceResponse> response = await _apiClient.RegisterDeviceAsync(request);
        if (response.Success)
        {
            _settings.SyncVpnDeviceId = deviceId;
            _settings.SyncVpnPushToken = pushToken;
            _settings.SyncVpnOneSignalSubscriptionId = oneSignalSubscriptionId;
        }
        else
        {
            Logger.Error<ApiErrorLog>($"Failed to register device with the new SyncVPN backend: {response.Error}");
        }
    }

    // TimeZoneInfo.Local.Id is a Windows-style name (e.g. "Pacific Standard Time"), but the backend
    // validates against the IANA tz database (e.g. "America/Los_Angeles") - sending the Windows name as-is
    // always failed its "must be a valid timezone" check. Falls back to the Windows id on the rare system
    // where ICU has no mapping, rather than sending an empty value.
    private static string GetIanaTimezoneId()
    {
        string windowsId = TimeZoneInfo.Local.Id;
        if (!TimeZoneInfo.TryConvertWindowsIdToIanaId(windowsId, out string? ianaId))
        {
            return windowsId;
        }

        // The backend's "timezone" validator rejects "Etc/UTC" (POST /devices/register returns 422
        // "The timezone field must be a valid timezone.") even though it's a standard IANA id - plain
        // "UTC" is what it actually accepts for a machine on UTC/GMT (Windows id "UTC").
        return ianaId == "Etc/UTC" ? "UTC" : ianaId;
    }
}
