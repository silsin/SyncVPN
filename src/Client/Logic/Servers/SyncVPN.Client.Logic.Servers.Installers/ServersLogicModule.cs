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
using SyncVPN.Client.Logic.Servers.Cache;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.FavoriteServers;
using SyncVPN.Client.Logic.Servers.Files;
using SyncVPN.Client.Logic.Servers.Loads;
using SyncVPN.Client.Logic.Servers.Mappers;
using SyncVPN.Client.Logic.Servers.Observers;
using SyncVPN.EntityMapping.Common.Installers.Extensions;

namespace SyncVPN.Client.Logic.Servers.Installers;

public class ServersLogicModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ServersLoader>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServersCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServersFileReaderWriter>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<DeviceLocationObserver>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<ServersObserver>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<ServersUpdater>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServerCountCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<FavoriteServersStorage>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServerFinder>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServerLoadsCalculator>().AsImplementedInterfaces().SingleInstance();
        
        builder.RegisterType<LocationNamesFileReaderWriter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LocationNamesProvider>().SingleInstance().AutoActivate();
        
        builder.RegisterAllMappersInAssembly<LogicalServerMapper>();
    }
}