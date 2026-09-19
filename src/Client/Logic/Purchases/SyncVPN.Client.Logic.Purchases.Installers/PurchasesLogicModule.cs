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
using SyncVPN.Client.Logic.Purchases;

namespace SyncVPN.Client.Logic.Purchases.Installers;

public class PurchasesLogicModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<FreeServersProvider>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<PlansProvider>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<BillingCountriesProvider>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<TransactionHistoryProvider>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServerFavoritesClient>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<PurchaseService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<CheckoutLinkService>().AsImplementedInterfaces().SingleInstance();
    }
}
