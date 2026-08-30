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

using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts.Preferences;
using SyncVPN.Client.Logic.Servers.Contracts.Enums;
using SyncVPN.Client.Logic.Servers.Contracts.Extensions;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts.Models;

namespace SyncVPN.Client.Logic.Connection.Preferences;

public class ExclusionChecker : IExclusionChecker,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;
    private volatile HashSet<ExcludedLocation> _excludedLocations = [];

    public bool HasExcludedLocations => _excludedLocations.Count > 0;

    public ExclusionChecker(ISettings settings)
    {
        _settings = settings;
    }

    public void Receive(LoggedInMessage message)
    {
        InvalidateExcludedLocations();
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.ExcludedLocationsList))
        {
            InvalidateExcludedLocations();
        }
    }

    private void InvalidateExcludedLocations()
    {
        HashSet<ExcludedLocation> newSet = new(_settings.ExcludedLocationsList);
        _excludedLocations = newSet;
    }

    public List<ExcludedLocation> GetExcludedLocations()
    {
        return _excludedLocations.ToList();
    }

    public bool IsCountryExcluded(Country country)
    {
        return IsCountryExcluded(country.Code);
    }

    public bool IsStateExcluded(State state)
    {
        return IsStateExcluded(state.CountryCode, state.Name);
    }

    public bool IsCityExcluded(City city)
    {
        return IsCityExcluded(city.CountryCode, city.StateName, city.Name);
    }

    public bool IsServerExcluded(Server server)
    {
        if (server.Features.IsSupported(ServerFeatures.SecureCore) &&
            IsCountryExcluded(server.EntryCountry))
        {
            return true;
        }

        return IsCountryExcluded(server.ExitCountry)
            || IsStateExcluded(server.ExitCountry, server.State)
            || IsCityExcluded(server.ExitCountry, server.State, server.City);
    }

    private bool IsCountryExcluded(string countryCode)
    {
        ExcludedLocation location = new(countryCode);
        return IsLocationExcluded(location);
    }

    private bool IsStateExcluded(string countryCode, string stateName)
    {
        ExcludedLocation location = new(countryCode, stateName);
        return IsLocationExcluded(location);
    }

    private bool IsCityExcluded(string countryCode, string? stateName, string cityName)
    {
        ExcludedLocation location = new(countryCode, stateName, cityName);
        return IsLocationExcluded(location);
    }

    private bool IsLocationExcluded(ExcludedLocation location)
    {
        return _excludedLocations.Contains(location);
    }
}
