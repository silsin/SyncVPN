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

using Autofac;
using SyncVPN.Client.Logic.Announcements.EntityMapping;
using SyncVPN.Client.Logic.Announcements.Files;
using SyncVPN.Client.Logic.Announcements.Images;
using SyncVPN.Client.Logic.Announcements.Observers;
using SyncVPN.EntityMapping.Common.Installers.Extensions;

namespace SyncVPN.Client.Logic.Announcements.Installers;

public class AnnouncementsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AnnouncementsFileReaderWriter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AnnouncementsProvider>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AnnouncementsObserver>().AsImplementedInterfaces().AutoActivate().SingleInstance();

        builder.RegisterType<AnnouncementImagesDeleter>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterAllMappersInAssembly<AnnouncementMapper>();
    }
}