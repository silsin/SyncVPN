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

namespace SyncVPN.Api.Contracts.Geographical;

public class ServerLocationResponse : LocationResponseBase
{
    private float _latitude;
    private float _longitude;

    public float Latitude
    {
        get => _latitude;
        set => _latitude = GetFilteredLatitude(value);
    }

    public float Longitude
    {
        get => _longitude;
        set => _longitude = GetFilteredLongitude(value);
    }
}