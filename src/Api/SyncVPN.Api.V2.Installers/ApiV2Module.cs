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
using SyncVPN.Api.BackendSelection;
using SyncVPN.Api.V2.Contracts;

namespace SyncVPN.Api.V2.Installers;

// Registered alongside (not instead of) ApiModule (Proton) - see the backend migration plan.
// IApiClient's public surface and its single Autofac registration are untouched by this module;
// migrated ApiClient methods branch internally via IBackendModeProvider to call ISyncVpnApiClient instead.
public class ApiV2Module : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SyncVpnApiClient>().As<ISyncVpnApiClient>().SingleInstance();
        builder.RegisterType<SyncVpnApiHostProvider>().As<ISyncVpnApiHostProvider>().SingleInstance();
        builder.RegisterType<SyncVpnAppTokenProvider>().As<ISyncVpnAppTokenProvider>().SingleInstance();
        builder.RegisterType<BackendModeProvider>().As<IBackendModeProvider>().SingleInstance();
    }
}
