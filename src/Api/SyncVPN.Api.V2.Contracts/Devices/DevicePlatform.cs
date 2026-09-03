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

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SyncVPN.Api.V2.Contracts.Devices;

// The API's OpenAPI schema declares "platform" as a string enum (windows/android/ios/macos/linux), not
// a number - StringEnumConverter is required or Newtonsoft serializes this as a plain int (0) instead,
// which the API rejects. EnumMember supplies the lowercase wire value Newtonsoft's default PascalCase
// member name wouldn't otherwise produce.
// All five values are modeled (not just Windows) because responses like LoginResponse.Data.Devices list
// every device on the account - a user also running the Android/iOS app would otherwise fail to
// deserialize their own login response on this Windows client.
[JsonConverter(typeof(StringEnumConverter))]
public enum DevicePlatform
{
    [EnumMember(Value = "windows")]
    Windows,

    [EnumMember(Value = "android")]
    Android,

    [EnumMember(Value = "ios")]
    Ios,

    [EnumMember(Value = "macos")]
    MacOs,

    [EnumMember(Value = "linux")]
    Linux,
}
