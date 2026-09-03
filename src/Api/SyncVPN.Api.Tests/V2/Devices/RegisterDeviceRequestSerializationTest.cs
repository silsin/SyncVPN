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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SyncVPN.Api.V2.Contracts.Devices;

namespace SyncVPN.Api.Tests.V2.Devices;

// The API's /devices/register OpenAPI schema declares "platform" as a STRING enum
// (windows/android/ios/macos/linux), not a number. SyncVpnApiClient serializes requests with plain
// JsonConvert.SerializeObject (no custom settings) - confirming here that DevicePlatform actually
// produces the expected lowercase string, since Newtonsoft serializes enums as their numeric value by
// default unless told otherwise, and a mismatch here would 422 for every registration attempt.
[TestClass]
public class RegisterDeviceRequestSerializationTest
{
    [TestMethod]
    public void RegisterDeviceRequest_SerializesPlatformAsLowercaseString_MatchingApiContract()
    {
        RegisterDeviceRequest request = new() { DeviceId = "test-device-id", Platform = DevicePlatform.Windows };

        string json = JsonConvert.SerializeObject(request);
        JObject parsed = JObject.Parse(json);

        parsed["platform"]!.Type.Should().Be(JTokenType.String);
        parsed["platform"]!.Value<string>().Should().Be("windows");
    }

    [TestMethod]
    public void Device_DeserializesNonWindowsPlatforms_WithoutThrowing()
    {
        // A user's account can have devices on other platforms - LoginResponse.Data.Devices lists all of
        // them, so this Windows client must be able to deserialize its own login response regardless.
        const string json = """{"device_id":"phone-1","platform":"android","language":"en"}""";

        Device device = JsonConvert.DeserializeObject<Device>(json)!;

        device.Platform.Should().Be(DevicePlatform.Android);
    }
}
