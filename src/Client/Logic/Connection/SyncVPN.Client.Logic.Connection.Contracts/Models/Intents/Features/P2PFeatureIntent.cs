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

using SyncVPN.Client.Logic.Servers.Contracts.Enums;
using SyncVPN.Client.Logic.Servers.Contracts.Extensions;
using SyncVPN.Client.Logic.Servers.Contracts.Models;

namespace SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;

public class P2PFeatureIntent : FeatureIntentBase
{
    public override bool IsForPaidUsersOnly => true;

    public override bool IsSupported(Server server)
    {
        return server.Features.IsSupported(ServerFeatures.P2P);
    }

    public override string ToString()
    {
        return "P2P";
    }
}