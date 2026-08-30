/*
 * Copyright (c) 2023 Proton AG
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

using System;
using Autofac;
using SyncVPN.Api.Contracts;
using SyncVPN.Configurations.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Update.Config;
using SyncVPN.Update.Contracts;
using SyncVPN.Update.Contracts.Config;
using SyncVPN.Update.Files.Launchable;
using SyncVPN.Update.Updates;
using Module = Autofac.Module;

namespace SyncVPN.Update.Installers;

public class UpdateModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AppUpdates>().SingleInstance();
        builder.RegisterType<LaunchableFile>().As<ILaunchableFile>().SingleInstance();

        builder.Register(c =>
            new CleanableOnceAppUpdates(
                new AsyncAppUpdates(
                    new SafeAppUpdates(c.Resolve<ILogger>(),
                        c.Resolve<AppUpdates>())
                ))).As<IAppUpdates>().SingleInstance();

        builder.Register(c =>
            new SafeAppUpdate(c.Resolve<ILogger>(),
                new ExtendedProgressAppUpdate(c.Resolve<IAppUpdateConfig>().MinProgressDuration,
                    new NotifyingAppUpdate(
                        new AppUpdate(c.Resolve<AppUpdates>()), c.Resolve<ILogger>()
                    )))).As<INotifyingAppUpdate>().SingleInstance();

        builder.Register(CreateDefaultAppUpdateConfig).As<IAppUpdateConfig>().SingleInstance();
    }

    // TO DO: Refactor the code in order to delete this custom registration
    private DefaultAppUpdateConfig CreateDefaultAppUpdateConfig(IComponentContext c)
    {
        IUpdateHttpClientFactory updateHttpClientFactory = c.Resolve<IUpdateHttpClientFactory>();

        return new DefaultAppUpdateConfig
        {
            FeedHttpClient = updateHttpClientFactory.GetFeedHttpClient(),
            FileHttpClient = updateHttpClientFactory.GetUpdateDownloadHttpClient(),
            FeedUriProvider = c.Resolve<IFeedUrlProvider>(),
            UpdatesPath = c.Resolve<IStaticConfiguration>().UpdatesFolder,
            CurrentVersion = Version.Parse(c.Resolve<IConfiguration>().ClientVersion),
            EarlyAccessCategoryName = "EarlyAccess",
            MinProgressDuration = TimeSpan.FromSeconds(1.5)
        };
    }
}