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

using Autofac;
using SyncVPN.Client.Localization.Building;
using SyncVPN.Client.Localization.Languages;

namespace SyncVPN.Client.Localization.Installers;

public class LocalizationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<LocalizerFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LocalizationService>().AsImplementedInterfaces().AutoActivate().SingleInstance();
        builder.RegisterType<LocalizationProvider>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LanguageFactory>().AsImplementedInterfaces().SingleInstance();        
    }
}