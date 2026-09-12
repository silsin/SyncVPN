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

using SyncVPN.Client.Core.Bases.Models;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Common.Core.Networking;

namespace SyncVPN.Client.Models.Settings;

public class ProtocolItem : ModelBase
{
    public VpnProtocol Protocol { get; }

    public string Header => Localizer.GetVpnProtocol(Protocol);

    public bool IsSmartProtocol => Protocol == VpnProtocol.Smart;

    public bool IsWireGuardTlsProtocol => Protocol == VpnProtocol.WireGuardTls;

    public bool IsWireGuardUdpProtocol => Protocol == VpnProtocol.WireGuardUdp;

    public bool IsWireGuardTcpProtocol => Protocol == VpnProtocol.WireGuardTcp;

    public bool IsOpenVpnUdpProtocol => Protocol == VpnProtocol.OpenVpnUdp;

    public bool IsOpenVpnTcpProtocol => Protocol == VpnProtocol.OpenVpnTcp;

    public bool IsL2tpProtocol => Protocol == VpnProtocol.L2tp;

    public bool IsSstpProtocol => Protocol == VpnProtocol.Sstp;

    public ProtocolItem(
        ILocalizationProvider localizer,
        VpnProtocol vpnProtocol)
        : base(localizer)
    {
        Protocol = vpnProtocol;
    }
}
