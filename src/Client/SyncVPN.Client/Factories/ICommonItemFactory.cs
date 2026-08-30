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

using SyncVPN.Client.Common.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Models;
using SyncVPN.Client.Models.Settings;
using SyncVPN.Client.Models.Profiles;
using SyncVPN.Client.Settings.Contracts.Enums;
using SyncVPN.Common.Core.Networking;

namespace SyncVPN.Client.Factories;

public interface ICommonItemFactory
{
    ProtocolItem GetProtocol(VpnProtocol protocol);

    NetShieldModeItem GetNetShieldMode(NetShieldMode? netShieldMode);

    NetShieldModeItem GetNetShieldMode(bool isEnabled, NetShieldMode netShieldMode);

    NatTypeItem GetNatType(NatType natType);

    PortForwardingItem GetPortForwardingMode(bool isEnabled);

    FeatureItem GetFeature(Feature feature);

    ConnectAndGoModeItem GetConnectAndGoMode(ConnectAndGoMode? connectAndGoMode);

    ConnectAndGoModeItem GetConnectAndGoMode(bool isEnabled, ConnectAndGoMode connectAndGoMode);
}
