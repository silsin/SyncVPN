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
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;
using SyncVPN.Client.Logic.Profiles.Contracts.Models;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Enums;
using SyncVPN.Common.Core.Networking;

namespace SyncVPN.Client.Logic.Profiles;

public class DefaultProfilesProvider : IDefaultProfilesProvider
{
    private readonly ILocalizationProvider _localizer;
    private readonly ISettings _settings;

    public DefaultProfilesProvider(
        ILocalizationProvider localizer,
        ISettings settings)
    {
        _localizer = localizer;
        _settings = settings;
    }

    public List<IConnectionProfile> GetDefaultProfiles()
    {
        return _settings.VpnPlan.IsB2B
            ? []
            : [ GetDefaultStreamingProfile(),
                GetDefaultGamingProfile(),
                GetDefaultP2PProfile(),
                GetDefaultMaxSecurityProfile(),
                GetDefaultWorkSchoolProfile(),
                GetDefaultRandomConnectionProfile() ];
    }

    private IConnectionProfile GetDefaultStreamingProfile()
    {
        ILocationIntent locationIntent = new SingleCountryLocationIntent("US");

        IConnectionProfile profile = new ConnectionProfile(locationIntent)
        {
            Name = _localizer.Get("Profiles_Default_Streaming"),
        };
        profile.Icon.Category = ProfileCategory.Streaming;

        return profile;
    }

    private IConnectionProfile GetDefaultGamingProfile()
    {
        ILocationIntent locationIntent = MultiCountryLocationIntent.Fastest;
        IFeatureIntent featureIntent = new P2PFeatureIntent();

        IConnectionProfile profile = new ConnectionProfile(locationIntent, featureIntent)
        {
            Name = _localizer.Get("Profiles_Default_Gaming"),
        };
        profile.Icon.Category = ProfileCategory.Gaming;
        profile.Settings.NatType = NatType.Moderate;

        return profile;
    }

    private IConnectionProfile GetDefaultP2PProfile()
    {
        ILocationIntent locationIntent = MultiCountryLocationIntent.Fastest;
        IFeatureIntent featureIntent = new P2PFeatureIntent();

        IConnectionProfile profile = new ConnectionProfile(locationIntent, featureIntent)
        {
            Name = _localizer.Get("Profiles_Default_P2P"),
        };
        profile.Icon.Category = ProfileCategory.Download;
        profile.Settings.IsPortForwardingEnabled = true;

        return profile;
    }

    private IConnectionProfile GetDefaultMaxSecurityProfile()
    {
        ILocationIntent locationIntent = MultiCountryLocationIntent.Fastest;
        IFeatureIntent featureIntent = new SecureCoreFeatureIntent();

        IConnectionProfile profile = new ConnectionProfile(locationIntent, featureIntent)
        {
            Name = _localizer.Get("Profiles_Default_MaxSecurity"),
        };
        profile.Icon.Category = ProfileCategory.Protection;

        return profile;
    }

    private IConnectionProfile GetDefaultWorkSchoolProfile()
    {
        ILocationIntent locationIntent = MultiCountryLocationIntent.Fastest;

        IConnectionProfile profile = new ConnectionProfile(locationIntent)
        {
            Name = _localizer.Get("Profiles_Default_WorkSchool"),
        };
        profile.Icon.Category = ProfileCategory.Business;
        profile.Settings.VpnProtocol = VpnProtocol.WireGuardTls;
        profile.Settings.IsNetShieldEnabled = true;
        profile.Settings.NetShieldMode = NetShieldMode.BlockMalwareOnly;
        profile.Settings.NatType = NatType.Moderate;

        return profile;
    }

    private IConnectionProfile GetDefaultRandomConnectionProfile()
    {
        ILocationIntent locationIntent = MultiCountryLocationIntent.Random;

        IConnectionProfile profile = new ConnectionProfile(locationIntent)
        {
            Name = _localizer.Get("Profiles_Default_RandomConnection"),
        };
        profile.Icon.Category = ProfileCategory.Browsing;

        return profile;
    }
}