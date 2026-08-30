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

using SyncVPN.Client.Contracts.Profiles;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Profiles.Contracts;
using SyncVPN.Client.Logic.Profiles.Contracts.Models;
using SyncVPN.Client.Logic.Recents.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Models.Connections.Profiles;
using SyncVPN.Client.Models.Connections.Recents;
using SyncVPN.Client.Settings.Contracts;

namespace SyncVPN.Client.Factories;

public class ConnectionItemFactory : IConnectionItemFactory
{
    private readonly ILocalizationProvider _localizer;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IServersLoader _serversLoader;
    private readonly IConnectionManager _connectionManager;
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly IProfilesManager _profilesManager;
    private readonly IProfileEditor _profileEditor;
    private readonly IUpsellCarouselWindowActivator _upsellCarouselWindowActivator;
    private readonly ISettings _settings;

    public ConnectionItemFactory(
        ILocalizationProvider localizer,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IServersLoader serversLoader,
        IConnectionManager connectionManager,
        IRecentConnectionsManager recentConnectionsManager,
        IProfilesManager profilesManager,
        IProfileEditor profileEditor,
        IUpsellCarouselWindowActivator upsellCarouselWindowActivator,
        ISettings settings)
    {
        _localizer = localizer;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _serversLoader = serversLoader;
        _connectionManager = connectionManager;
        _recentConnectionsManager = recentConnectionsManager;
        _profilesManager = profilesManager;
        _profileEditor = profileEditor;
        _upsellCarouselWindowActivator = upsellCarouselWindowActivator;
        _settings = settings;
    }

    public RecentConnectionItem GetRecent(IRecentConnection recentConnection)
    {
        return new RecentConnectionItem(_localizer, _serversLoader, _connectionManager, _upsellCarouselWindowActivator, _settings, _recentConnectionsManager, recentConnection);
    }

    public ProfileConnectionItem GetProfile(IConnectionProfile profile)
    {
        return new ProfileConnectionItem(_localizer, _serversLoader, _connectionManager, _upsellCarouselWindowActivator, _mainWindowOverlayActivator, _profilesManager, _profileEditor, profile);
    }
}