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

using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Connection.Contracts.RequestCreators;
using SyncVPN.Client.Logic.Profiles.Contracts.Models;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Enums;
using SyncVPN.Client.Settings.Contracts.Models;
using SyncVPN.Common.Core.Networking;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Dns;
using SyncVPN.ProcessCommunication.Contracts.Entities.Settings;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace SyncVPN.Client.Logic.Connection.RequestCreators;

public class MainSettingsRequestCreator : IMainSettingsRequestCreator
{
    private readonly ISettings _settings;
    private readonly IEntityMapper _entityMapper;

    public MainSettingsRequestCreator(
        ISettings settings,
        IEntityMapper entityMapper)
    {
        _settings = settings;
        _entityMapper = entityMapper;
    }

    public MainSettingsIpcEntity Create(IConnectionIntent? connectionIntent)
    {
        MainSettingsIpcEntity settings = Create();

        if (connectionIntent is IConnectionProfile connectionProfile)
        {
            settings.VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(connectionProfile.Settings.VpnProtocol);
            settings.NetShieldMode = connectionProfile.Settings.IsNetShieldEnabled ? (int)connectionProfile.Settings.NetShieldMode : 0;
            settings.PortForwarding = connectionProfile.Settings.IsPortForwardingEnabled;
            settings.ModerateNat = connectionProfile.Settings.NatType == NatType.Moderate;
        }

        if (settings.NetShieldMode == (int)NetShieldMode.BlockAdsMalwareTrackersAdultContent && 
            (_settings.VpnPlan.IsB2B || connectionIntent?.Feature is B2BFeatureIntent))
        {
            settings.NetShieldMode = (int)NetShieldMode.BlockAdsMalwareTrackers;
        }

        return settings;
    }

    private MainSettingsIpcEntity Create()
    {
        return new MainSettingsIpcEntity
        {
            VpnProtocol = _entityMapper.Map<VpnProtocol, VpnProtocolIpcEntity>(_settings.VpnProtocol),
            KillSwitchMode = _settings.IsKillSwitchEnabled
                ? _entityMapper.Map<KillSwitchMode, KillSwitchModeIpcEntity>(_settings.KillSwitchMode)
                : KillSwitchModeIpcEntity.Off,
            SplitTunnel = new SplitTunnelSettingsIpcEntity
            {
                Mode = _settings.IsSplitTunnelingEnabled
                    ? _entityMapper.Map<SplitTunnelingMode, SplitTunnelModeIpcEntity>(_settings.SplitTunnelingMode)
                    : SplitTunnelModeIpcEntity.Disabled,
                AppPaths = GetSplitTunnelingApps(),
                Ips = GetSplitTunnelingIpAddresses()
            },
            ModerateNat = _settings.NatType == NatType.Moderate,
            NetShieldMode = _settings.IsNetShieldEnabled ? (int)_settings.NetShieldMode : 0,
            Ipv6LeakProtection = _settings.IsIpv6LeakProtectionEnabled,
            IsIpv6Enabled = _settings.IsIpv6Enabled,
            Ipv6Fragments = _settings.Ipv6Fragments,
            IsShareCrashReportsEnabled = _settings.IsShareCrashReportsEnabled,
            IsLocalAreaNetworkAccessEnabled = _settings.IsLocalAreaNetworkAccessEnabled,
            PortForwarding = _settings.IsPortForwardingEnabled,
            SplitTcp = _settings.IsVpnAcceleratorEnabled,
            OpenVpnAdapter = _entityMapper.Map<OpenVpnAdapter, OpenVpnAdapterIpcEntity>(_settings.OpenVpnAdapter),
            WireGuardConnectionTimeout = _settings.WireGuardConnectionTimeout,
            DnsBlockMode = _settings.IsLocalAreaNetworkAccessEnabled && _settings.IsLocalDnsEnabled
                ? DnsBlockModeIpcEntity.Callout
                : DnsBlockModeIpcEntity.Nrpt,
            ShouldDisableWeakHostSetting = true,
        };
    }

    private string[] GetSplitTunnelingApps()
    {
        return _settings.SplitTunnelingMode == SplitTunnelingMode.Standard
            ? GetSplitTunnelingApps(_settings.SplitTunnelingStandardAppsList)
            : GetSplitTunnelingApps(_settings.SplitTunnelingInverseAppsList);
    }

    private string[] GetSplitTunnelingApps(List<SplitTunnelingApp> settingsApps)
    {
        return settingsApps.Where(app => app.IsActive).SelectMany(app => app.GetAllAppFilePaths()).ToArray();
    }

    private string[] GetSplitTunnelingIpAddresses()
    {
        return _settings.SplitTunnelingMode == SplitTunnelingMode.Standard
            ? GetSplitTunnelingIpAddresses(_settings.SplitTunnelingStandardIpAddressesList)
            : GetSplitTunnelingIpAddresses(_settings.SplitTunnelingInverseIpAddressesList);
    }

    private string[] GetSplitTunnelingIpAddresses(List<SplitTunnelingIpAddress> settingsIpAddresses)
    {
        return settingsIpAddresses.Where(ip => ip.IsActive).Select(ip => ip.IpAddress).ToArray();
    }
}
