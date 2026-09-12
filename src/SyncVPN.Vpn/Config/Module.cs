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

using Autofac;
using SyncVPN.Common.Legacy.OS.Processes;
using SyncVPN.Common.Legacy.Threading;
using SyncVPN.Configurations.Contracts;
using SyncVPN.Configurations.Contracts.Entities;
using SyncVPN.Configurations.Contracts.WireGuard;
using SyncVPN.Crypto.Contracts;
using SyncVPN.IssueReporting.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.OperatingSystems.Network.Contracts;
using SyncVPN.OperatingSystems.Network.Contracts.Monitors;
using SyncVPN.OperatingSystems.Processes.Contracts;
using SyncVPN.OperatingSystems.Ras;
using SyncVPN.OperatingSystems.Ras.Contracts;
using SyncVPN.OperatingSystems.Services.Contracts;
using SyncVPN.Vpn.Common;
using SyncVPN.Vpn.Connection;
using SyncVPN.Vpn.Gateways;
using SyncVPN.Vpn.LocalAgent;
using SyncVPN.Vpn.Management;
using SyncVPN.Vpn.NetShield;
using SyncVPN.Vpn.NetworkAdapters;
using SyncVPN.Vpn.NRPT;
using SyncVPN.Vpn.OpenVpn;
using SyncVPN.Vpn.OpenVpn.DnsServers;
using SyncVPN.Vpn.PortMapping;
using SyncVPN.Vpn.PortMapping.Serializers.Common;
using SyncVPN.Vpn.PortMapping.UdpClients;
using SyncVPN.Vpn.PortScanning;
using SyncVPN.Vpn.Restrictions;
using SyncVPN.Vpn.ServerValidation;
using SyncVPN.Vpn.SplitTunnel;
using SyncVPN.Vpn.SynchronizationEvent;
using SyncVPN.Vpn.WireGuard;

namespace SyncVPN.Vpn.Config;

public class Module
{
    public void Load(ContainerBuilder builder)
    {
        builder.RegisterType<LocalAgentTlsCredentialsCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServerValidator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<GatewayCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<DnsServerCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<OpenVpnDnsServersCreator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<VpnEndpointScanner>().SingleInstance();
        builder.RegisterType<TcpPortScanner>().SingleInstance();
        builder.RegisterType<SplitTunnelRouting>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UdpPingClient>().SingleInstance();
        builder.RegisterType<WintunAdapter>().SingleInstance();
        builder.RegisterType<TapAdapter>().SingleInstance();
        builder.RegisterType<NetShieldStatisticEventManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NrptWrapper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<RestrictionsEventManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<WireGuardServerRouteManager>().AsImplementedInterfaces().SingleInstance();
        builder.Register(c =>
            {
                ILogger logger = c.Resolve<ILogger>();
                IStaticConfiguration staticConfig = c.Resolve<IStaticConfiguration>();

                return new OpenVpnProcess(
                    logger,
                    c.Resolve<IOsProcesses>(),
                    new OpenVpnExitEvent(logger,
                        new SystemSynchronizationEvents(logger),
                        staticConfig.OpenVpn.ExitEventName),
                    staticConfig);
            }
        ).SingleInstance();

        RegisterPortMapping(builder);
    }

    private void RegisterPortMapping(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IMessageSerializer).Assembly)
            .Where(t => typeof(IMessageSerializer).IsAssignableFrom(t))
            .AsImplementedInterfaces()
            .SingleInstance();
        builder.RegisterType<MessageSerializerFactory>().As<IMessageSerializerFactory>().SingleInstance();
        builder.RegisterType<MessageSerializerProxy>().As<IMessageSerializerProxy>().SingleInstance();
        builder.RegisterType<UdpClientWrapper>().As<IUdpClientWrapper>().SingleInstance();
        builder.RegisterType<PortMappingProtocolClient>().As<IPortMappingProtocolClient>().SingleInstance();
    }

    public IVpnConnection GetVpnConnection(IComponentContext c)
    {
        ILogger logger = c.Resolve<ILogger>();
        INetworkInterfaceLoader networkInterfaceLoader = c.Resolve<INetworkInterfaceLoader>();
        ITaskQueue taskQueue = c.Resolve<ITaskQueue>();
        TcpPortScanner tcpPortScanner = c.Resolve<TcpPortScanner>();
        tcpPortScanner.Config(c.Resolve<IStaticConfiguration>().OpenVpn.StaticKey);
        IEndpointScanner endpointScanner = c.Resolve<VpnEndpointScanner>();
        VpnEndpointCandidates candidates = new();
        IIssueReporter issueReporter = c.Resolve<IIssueReporter>();
        IPortMappingProtocolClient portMappingProtocolClient = c.Resolve<IPortMappingProtocolClient>();
        IServerValidator serverValidator = c.Resolve<IServerValidator>();
        INrptWrapper nrptWrapper = c.Resolve<INrptWrapper>();

        return new LoggingWrapper(
            logger,
                new ReconnectingWrapper(
                    logger,
                    candidates,
                    serverValidator,
                    endpointScanner,
                    c.Resolve<ILocalAgentTlsCredentialsCache>(),
                    new HandlingRequestsWrapper(
                        logger,
                        taskQueue,
                        new ServerAuthenticatorWrapper(
                            serverValidator,
                            new BestPortWrapper(
                                logger,
                                taskQueue,
                                endpointScanner,
                                new NetworkAdapterStatusWrapper(
                                    logger,
                                    issueReporter,
                                    networkInterfaceLoader,
                                    c.Resolve<WintunAdapter>(),
                                    c.Resolve<TapAdapter>(),
                                    nrptWrapper,
                                    new QueueingEventsWrapper(
                                        taskQueue,
                                        new PortForwardingWrapper(
                                            logger,
                                            portMappingProtocolClient,
                                            new VpnProtocolWrapper(GetOpenVpnConnection(c), GetWireguardConnection(c),
                                                GetL2tpConnection(c), GetSstpConnection(c))))))))));
    }

    private ISingleVpnConnection GetWireguardConnection(IComponentContext c)
    {
        ILogger logger = c.Resolve<ILogger>();
        IStaticConfiguration staticConfig = c.Resolve<IStaticConfiguration>();
        IConfiguration config = c.Resolve<IConfiguration>();
        IGatewayCache gatewayCache = c.Resolve<IGatewayCache>();
        INetShieldStatisticEventManager netShieldStatisticEventManager = c.Resolve<INetShieldStatisticEventManager>();
        IRestrictionsEventManager restrictionsEventManager = c.Resolve<IRestrictionsEventManager>();
        IX25519KeyGenerator x25519KeyGenerator = c.Resolve<IX25519KeyGenerator>();
        ILocalAgentTlsCredentialsCache localAgentTlsCredentialsCache = c.Resolve<ILocalAgentTlsCredentialsCache>();
        IWireGuardDnsServersCreator wireGuardDnsServersCreator = c.Resolve<IWireGuardDnsServersCreator>();
        IServiceFactory serviceFactory = c.Resolve<IServiceFactory>();
        IWireGuardServerRouteManager wireGuardServerRouteManager = c.Resolve<IWireGuardServerRouteManager>();

        return new LocalAgentWrapper(logger, new EventReceiver(logger, netShieldStatisticEventManager, restrictionsEventManager), c.Resolve<ISplitTunnelRouting>(),
            gatewayCache,
            localAgentTlsCredentialsCache,
            new WireGuardConnection(logger, config, gatewayCache,
                c.Resolve<ISystemNetworkInterfaces>(),
                c.Resolve<IInterfaceForwardingMonitor>(),
                c.Resolve<IRouteChangeMonitor>(),
                c.Resolve<INetworkInterfacePolicyManager>(),
                new WireGuardService(logger, staticConfig, serviceFactory.Get(staticConfig.WireGuard.ServiceName)),
                new WireGuardConfigGenerator(config, x25519KeyGenerator, wireGuardDnsServersCreator),
                new NtTrafficManager(staticConfig.WireGuard.ConfigFileName, logger),
                new WintunTrafficManager(staticConfig.WireGuard.PipeName),
                new StatusManager(logger, staticConfig.WireGuard.LogFilePath),
                wireGuardServerRouteManager));
    }

    private ISingleVpnConnection GetL2tpConnection(IComponentContext c)
    {
        ILogger logger = c.Resolve<ILogger>();
        return new L2tpConnection(logger, () => new RasConnection(logger));
    }

    private ISingleVpnConnection GetSstpConnection(IComponentContext c)
    {
        ILogger logger = c.Resolve<ILogger>();
        return new SstpConnection(logger, () => new RasConnection(logger));
    }

    private ISingleVpnConnection GetOpenVpnConnection(IComponentContext c)
    {
        ILogger logger = c.Resolve<ILogger>();
        IOpenVpnConfigurations openVpnConfig = c.Resolve<IStaticConfiguration>().OpenVpn;
        IGatewayCache gatewayCache = c.Resolve<IGatewayCache>();
        IDnsServerCache dnsServerCache = c.Resolve<IDnsServerCache>();
        INetShieldStatisticEventManager netShieldStatisticEventManager = c.Resolve<INetShieldStatisticEventManager>();
        ILocalAgentTlsCredentialsCache localAgentTlsCredentialsCache = c.Resolve<ILocalAgentTlsCredentialsCache>();
        IRestrictionsEventManager restrictionsEventManager = c.Resolve<IRestrictionsEventManager>();

        return new LocalAgentWrapper(logger, new EventReceiver(logger, netShieldStatisticEventManager, restrictionsEventManager), c.Resolve<ISplitTunnelRouting>(),
            gatewayCache,
            localAgentTlsCredentialsCache,
            new OpenVpnConnection(
                logger,
                c.Resolve<IStaticConfiguration>(),
                c.Resolve<INetworkInterfaceLoader>(),
                c.Resolve<OpenVpnProcess>(),
                c.Resolve<IRandomStringGenerator>(),
                c.Resolve<ICommandLineCaller>(),
                new ManagementClient(
                    logger,
                    gatewayCache,
                    dnsServerCache,
                    new ConcurrentManagementChannel(
                        new TcpManagementChannel(
                            logger,
                            openVpnConfig.ManagementHost)))));
    }
}