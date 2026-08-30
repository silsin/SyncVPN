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

using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.GuestHole;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.Client.Logic.Connection.Contracts.RequestCreators;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Observers;
using SyncVPN.Common.Legacy.Vpn;
using SyncVPN.Configurations.Contracts;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Settings;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace SyncVPN.Client.Logic.Connection.RequestCreators;

public class GuestHoleConnectionRequestCreator : ConnectionRequestCreatorBase, IGuestHoleConnectionRequestCreator
{
    private readonly IConfiguration _config;
    private readonly IConnectionKeyManager _connectionKeyManager;

    public GuestHoleConnectionRequestCreator(
        IConfiguration config,
        ILogger logger,
        ISettings settings,
        IEntityMapper entityMapper,
        IConnectionKeyManager connectionKeyManager,
        IFeatureFlagsObserver featureFlagsObserver,
        IMainSettingsRequestCreator mainSettingsRequestCreator)
        : base(logger, settings, entityMapper, featureFlagsObserver, mainSettingsRequestCreator)
    {
        _config = config;
        _connectionKeyManager = connectionKeyManager;
    }

    public async Task<ConnectionRequestIpcEntity> CreateAsync(IEnumerable<GuestHoleServerContract> servers)
    {
        MainSettingsIpcEntity settings = GetSettings();
        settings.OpenVpnAdapter = OpenVpnAdapterIpcEntity.Tap;
        settings.VpnProtocol = VpnProtocolIpcEntity.Smart;

        ConnectionRequestIpcEntity request = new()
        {
            RetryId = Guid.NewGuid(),
            Config = GetVpnConfig(settings),
            Credentials = await GetVpnCredentialsAsync(),
            Protocol = VpnProtocolIpcEntity.Smart,
            Servers = GetVpnServers(servers),
            Settings = settings,
        };

        return request;
    }

    protected override VpnConfigIpcEntity GetVpnConfig(MainSettingsIpcEntity settings, IConnectionIntent? connectionIntent = null)
    {
        return new()
        {
            VpnProtocol = settings.VpnProtocol,
            PreferredProtocols = [
                VpnProtocolIpcEntity.WireGuardTls,
                VpnProtocolIpcEntity.OpenVpnTcp,
            ],
            Ports = {
                { VpnProtocolIpcEntity.WireGuardTls, Settings.WireGuardTlsPorts },
                { VpnProtocolIpcEntity.OpenVpnTcp, Settings.OpenVpnTcpPorts },
            },
            IsWireGuardServerRouteEnabled = true,
        };
    }

    protected override Task<VpnCredentialsIpcEntity> GetVpnCredentialsAsync()
    {
        VpnCredentialsIpcEntity credentials = EntityMapper.Map<VpnCredentials, VpnCredentialsIpcEntity>(
            new VpnCredentials(
                _connectionKeyManager.GenerateTemporaryKeyPair(),
                AddSuffixToUsername(_config.GuestHoleVpnUsername),
                _config.GuestHoleVpnPassword));

        return Task.FromResult(credentials);
    }

    private string AddSuffixToUsername(string username)
    {
        return username + _config.VpnUsernameSuffix;
    }

    private VpnServerIpcEntity[] GetVpnServers(IEnumerable<GuestHoleServerContract> servers)
    {
        return servers
            .Select(s => EntityMapper.Map<GuestHoleServerContract, VpnServerIpcEntity>(s))
            .Where(s => s is not null)
            .ToArray();
    }
}