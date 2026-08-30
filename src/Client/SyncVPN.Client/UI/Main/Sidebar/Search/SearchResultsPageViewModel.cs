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

using CommunityToolkit.Mvvm.ComponentModel;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Logic.Searches.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Enums;
using SyncVPN.Client.Logic.Servers.Contracts.Extensions;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Logic.Servers.Contracts.Models;
using SyncVPN.Client.Logic.Servers.Contracts.Searches;
using SyncVPN.Client.Models.Connections;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Main.Sidebar.Bases;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;
using SyncVPN.Client.UI.Main.Sidebar.Search.Contracts;

namespace SyncVPN.Client.UI.Main.Sidebar.Search;

public partial class SearchResultsPageViewModel : ConnectionListViewModelBase<ISidebarViewNavigator>,
    ISearchInputReceiver, 
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<NewServerFoundMessage>,
    IEventMessageReceiver<LocationNamesChangedMessage>
{
    private readonly IGlobalSearch _globalSearch;
    private readonly ILocationItemFactory _locationItemFactory;
    private readonly IServerFinder _serverFinder;

    private string _input = string.Empty;

    [ObservableProperty]
    private bool _hasSearchInput;

    [ObservableProperty]
    private ICountriesComponent _selectedCountriesComponent;

    public List<ICountriesComponent> CountriesComponents { get; }

    public string ExampleCountries => $"{Localizer.GetCountryName("JP")}, {Localizer.GetCountryName("US")}";
    public string ExampleCities => $"{Localizer.GetCityName("Tokyo", "JP")}, {Localizer.GetCityName("Los Angeles", "US")}";
    public string ExampleServers => "JP#75, US-NY#166";

    public SearchResultsPageViewModel(
        ISettings settings,
        IConnectionManager connectionManager,
        IServersLoader serversLoader,
        ISidebarViewNavigator parentViewNavigator,
        IGlobalSearch globalSearch,
        ILocationItemFactory locationItemFactory,
        IConnectionGroupFactory connectionGroupFactory,
        IEnumerable<ICountriesComponent> countriesComponents,
        IViewModelHelper viewModelHelper,
        IServerFinder serverFinder)
        : base(parentViewNavigator,
               settings,
               serversLoader,
               connectionManager,
               connectionGroupFactory,
               viewModelHelper)
    {
        _globalSearch = globalSearch;
        _locationItemFactory = locationItemFactory;
        _serverFinder = serverFinder;

        CountriesComponents = new(countriesComponents.OrderBy(p => p.SortIndex));
        _selectedCountriesComponent = CountriesComponents.First();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        OnPropertyChanged(nameof(ExampleCountries));
        SearchAsync().Wait();
    }

    partial void OnSelectedCountriesComponentChanged(ICountriesComponent value)
    {
        SearchAsync().Wait();
    }

    public async Task SearchAsync(string input)
    {
        _input = input;
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        string input = _input;
        if (string.IsNullOrWhiteSpace(input))
        {
            HasSearchInput = false;
            SetSearchResult([]);
            _serverFinder.Cancel();
        }
        else
        {
            HasSearchInput = true;
            IEnumerable<ConnectionItemBase> result = await SetSearchResultsAsync(input);
            TriggerServerSearchTimerIfNecessary(input, result);
        }
    }

    private async Task<IEnumerable<ConnectionItemBase>> SetSearchResultsAsync(string input)
    {
        IEnumerable<ConnectionItemBase> result = (await _globalSearch.SearchAsync(input, GetServerFeatures()))
            .Select(GetConnectionItemCreationFunction())
            .Where(ci => ci is not null)
            .Cast<ConnectionItemBase>();
        SetSearchResult(result);

        return result;
    }

    private void TriggerServerSearchTimerIfNecessary(string input, IEnumerable<ConnectionItemBase> result)
    {
        if (!result.Where(r => r is ServerLocationItemBase slib && DoesInputMatchServerName(input, slib.Server.Name)).Any())
        {
            _serverFinder.Search(input);
        }
        else
        {
            _serverFinder.Cancel();
        }
    }

    private bool DoesInputMatchServerName(string input, string serverName)
    {
        return string.Equals(TrimServerName(input), TrimServerName(serverName), StringComparison.InvariantCultureIgnoreCase);
    }

    private string TrimServerName(string input)
    {
        return input.Replace("#", "").Replace("-", "").Replace(" ", "");
    }

    private ServerFeatures? GetServerFeatures()
    {
        return SelectedCountriesComponent.ConnectionType switch
        {
            CountriesConnectionType.SecureCore => ServerFeatures.SecureCore,
            CountriesConnectionType.P2P => ServerFeatures.P2P,
            CountriesConnectionType.Tor => ServerFeatures.Tor,
            _ => null,
        };
    }

    private void SetSearchResult(IEnumerable<ConnectionItemBase> result)
    {
        ResetItems(result);
        ResetGroups();

        InvalidateActiveConnection();
        InvalidateMaintenanceStates();
        InvalidateRestrictions();

        OnPropertyChanged(nameof(HasItems));
    }

    private Func<ILocation, ConnectionItemBase?> GetConnectionItemCreationFunction()
    {
        return SelectedCountriesComponent.ConnectionType switch
        {
            CountriesConnectionType.SecureCore => CreateSecureCoreConnectionItem,
            CountriesConnectionType.P2P => CreateP2PConnectionItem,
            CountriesConnectionType.Tor => CreateTorConnectionItem,
            _ => CreateStandardConnectionItem,
        };
    }

    private ConnectionItemBase? CreateSecureCoreConnectionItem(ILocation location)
    {
        if (location is Server server)
        {
            return _locationItemFactory.GetServer(server, isSearchItem: true);
        }
        else if (location is Country country)
        {
            return _locationItemFactory.GetSecureCoreCountry(country, isSearchItem: true);
        }

        return null;
    }

    private ConnectionItemBase? CreateP2PConnectionItem(ILocation location)
    {
        if (location is Server server)
        {
            return _locationItemFactory.GetP2PServer(server, isSearchItem: true);
        }
        else if (location is City city)
        {
            return _locationItemFactory.GetP2PCity(city, isSearchItem: true);
        }
        else if (location is State state)
        {
            return _locationItemFactory.GetP2PState(state, isSearchItem: true);
        }
        else if (location is Country country)
        {
            return _locationItemFactory.GetP2PCountry(country, isSearchItem: true);
        }

        return null;
    }

    private ConnectionItemBase? CreateTorConnectionItem(ILocation location)
    {
        if (location is Server server)
        {
            return _locationItemFactory.GetTorServer(server, isSearchItem: true);
        }
        else if (location is Country country)
        {
            return _locationItemFactory.GetTorCountry(country, isSearchItem: true);
        }

        return null;
    }

    private ConnectionItemBase? CreateStandardConnectionItem(ILocation location)
    {
        if (location is Server server)
        {
            if (server.Features.IsB2B())
            {
                return _locationItemFactory.GetGatewayServer(server);
            }
            else
            {
                return _locationItemFactory.GetServer(server, isSearchItem: true);
            }
        }
        else if (location is City city)
        {
            return _locationItemFactory.GetCity(city, isSearchItem: true);
        }
        else if (location is State state)
        {
            return _locationItemFactory.GetState(state, isSearchItem: true);
        }
        else if (location is Country country)
        {
            return _locationItemFactory.GetCountry(country, isSearchItem: true);
        }

        return null;
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateActiveConnection);
    }

    public void Receive(ServerListChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            InvalidateActiveConnection();
            InvalidateMaintenanceStates();
            InvalidateRestrictions();
        });
    }

    public void Receive(NewServerFoundMessage message)
    {
        string input = _input;
        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        ExecuteOnUIThread(async () =>
        {
            await SetSearchResultsAsync(input);
        });
    }

    public void Receive(LocationNamesChangedMessage message)
    {
        ExecuteOnUIThread(() => SearchAsync().Wait());
    }
}