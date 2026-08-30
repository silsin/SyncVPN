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
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Conflicts.Bases;
using SyncVPN.Client.Settings.Files;
using SyncVPN.Client.Settings.Initializers;
using SyncVPN.Client.Settings.Migrations;
using SyncVPN.Client.Settings.Observers;
using SyncVPN.Client.Settings.Repositories;
using SyncVPN.Client.Settings.RequiredReconnections;

namespace SyncVPN.Client.Settings.Installers;

public class SettingsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Settings>().As<ISettings>().SingleInstance();
        builder.RegisterType<GlobalSettings>().As<IGlobalSettings>().SingleInstance();
        builder.RegisterType<SessionSettings>().As<ISessionSettings>().SingleInstance();

        builder.RegisterType<UserSettingsFileReaderWriter>().As<IUserSettingsFileReaderWriter>().SingleInstance();
        builder.RegisterType<GlobalSettingsFileReaderWriter>().As<IGlobalSettingsFileReaderWriter>().SingleInstance();
        builder.RegisterType<GlobalSettingsCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UserSettingsCache>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<SettingsRestorer>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<GlobalSettingsMigrator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UserSettingsMigrator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ProfilesMigrator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SettingsCorrector>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ClientConfigObserver>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<FeatureFlagsObserver>().AsImplementedInterfaces().AutoActivate().SingleInstance();

        builder.RegisterType<SettingsConflictResolver>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<RequiredReconnectionSettings>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SystemConfigurationInitializer>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterAssemblyTypes(typeof(ISettingsConflict).Assembly)
               .Where(t => typeof(ISettingsConflict).IsAssignableFrom(t))
               .AsImplementedInterfaces()
               .SingleInstance();
    }
}