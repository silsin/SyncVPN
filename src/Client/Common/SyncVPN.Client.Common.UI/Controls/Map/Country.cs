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

using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;

namespace SyncVPN.Client.Common.UI.Controls.Map;

public class Country
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public double Longitude { get; set; } = 0;

    public double Latitude { get; set; } = 0;

    public bool IsUnderMaintenance { get; set; } = true;

    // Set for pins backed by the new SyncVPN backend's server catalog (free or Pro) rather than a
    // legacy Proton country - lets the connect handler target this exact server directly instead of
    // the country. Naming predates Pro servers also using this catalog; see FreeServerIsForPaidUsersOnly
    // for whether this specific pin still needs the paid-plan upsell gate.
    public bool IsFreeServer { get; set; } = false;

    public long FreeServerId { get; set; }

    public string FreeServerName { get; set; } = string.Empty;

    public string FreeServerProtocol { get; set; } = string.Empty;

    // True for a Pro-tier server pin - the connect handler must still gate these behind the paid-plan
    // upsell for a non-paid user, unlike a genuinely free server (Free == 1), which never gates.
    public bool FreeServerIsForPaidUsersOnly { get; set; } = false;

    public MPoint GetMapPoint()
    {
        return SphericalMercator.FromLonLat(Longitude, Latitude).ToMPoint();
    }
}