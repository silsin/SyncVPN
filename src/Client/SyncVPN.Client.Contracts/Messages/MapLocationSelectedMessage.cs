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

namespace SyncVPN.Client.Contracts.Messages;

// Sent the moment a free-server row is clicked (before the connect attempt resolves), so the map can pan
// to that exact server's coordinates immediately instead of waiting for a live ConnectionStatusChangedMessage
// - which, unlike the click itself, depends on the native VPN service actually reporting back.
public class MapLocationSelectedMessage
{
    public required double Latitude { get; init; }

    public required double Longitude { get; init; }

    public required string CountryCode { get; init; }

    // True only when a user deliberately picked this location from the Free servers list (as opposed to
    // this message being sent to pan the map to a server auto-picked by a "fastest server" connect) - see
    // MapComponentViewModel.Receive(MapLocationSelectedMessage), which uses this to decide whether to also
    // populate SelectedCountry (and so show the screen-level Connect button) rather than just panning.
    public bool IsExplicitSelection { get; init; }
}
