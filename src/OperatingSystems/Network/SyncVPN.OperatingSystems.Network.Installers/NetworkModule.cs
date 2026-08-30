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

using Autofac;
using SyncVPN.OperatingSystems.Network.Monitors;
using SyncVPN.OperatingSystems.Network.NetworkInterface;
using SyncVPN.OperatingSystems.Network.Policies;
using SyncVPN.OperatingSystems.Network.Routing;

namespace SyncVPN.OperatingSystems.Network.Installers;

public class NetworkModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ProxyDetector>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SystemNetworkInterfaces>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NetworkInterfaceLoader>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NetworkUtilities>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<RoutingTableHelper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<Ipv4GatewayResolver>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NetworkInterfacePolicyManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<InterfaceForwardingMonitor>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<RouteChangeMonitor>().AsImplementedInterfaces().SingleInstance();
    }
}