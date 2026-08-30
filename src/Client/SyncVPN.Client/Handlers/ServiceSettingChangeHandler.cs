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

using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Handlers.Bases;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;

namespace SyncVPN.Client.Handlers;

public class ServiceSettingChangeHandler : IHandler, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly Dictionary<string, Func<bool>> _settings;

    private readonly IVpnServiceSettingsUpdater _vpnServiceSettingsUpdater;

    public ServiceSettingChangeHandler(
        IVpnServiceSettingsUpdater vpnServiceSettingsUpdater,
        ISettings settings)
    {
        _vpnServiceSettingsUpdater = vpnServiceSettingsUpdater;

        _settings = new()
        {
            {nameof(ISettings.IsKillSwitchEnabled), () => true},
            {nameof(ISettings.KillSwitchMode), () => settings.IsKillSwitchEnabled},
            {nameof(ISettings.IsNetShieldEnabled), () => true},
            {nameof(ISettings.NetShieldMode), () => settings.IsNetShieldEnabled},
            {nameof(ISettings.IsPortForwardingEnabled), () => true},
            {nameof(ISettings.IsVpnAcceleratorEnabled), () => true},
            {nameof(ISettings.NatType), () => true},
            {nameof(ISettings.IsShareCrashReportsEnabled), () => true},
            {nameof(ISettings.IsLocalAreaNetworkAccessEnabled), () => true},
            {nameof(ISettings.IsIpv6Enabled), () => true},
        };
    }

    public async void Receive(SettingChangedMessage message)
    {
        if (_settings.ContainsKey(message.PropertyName) && _settings[message.PropertyName]())
        {
            await _vpnServiceSettingsUpdater.SendAsync();
        }
    }
}