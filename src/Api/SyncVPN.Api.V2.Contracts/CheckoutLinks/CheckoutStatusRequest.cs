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

using Newtonsoft.Json;

namespace SyncVPN.Api.V2.Contracts.CheckoutLinks;

// Body for POST /checkout-links/status. Sent as a POST body (not a query string) specifically so
// poll_token never ends up in an access log - same reasoning as WebAppLoginStatusRequest.
public class CheckoutStatusRequest
{
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("poll_token")]
    public string PollToken { get; set; } = string.Empty;
}
