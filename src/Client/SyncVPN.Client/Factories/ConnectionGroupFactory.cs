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

using SyncVPN.Client.Contracts.Enums;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Models.Connections;

namespace SyncVPN.Client.Factories;

public class ConnectionGroupFactory : IConnectionGroupFactory
{
    private readonly ILocalizationProvider _localizer;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;

    public ConnectionGroupFactory(
        ILocalizationProvider localizer,
        IMainWindowOverlayActivator mainWindowOverlayActivator)
    {
        _localizer = localizer;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
    }

    public ConnectionGroup GetGroup(ConnectionGroupType groupType, IEnumerable<ConnectionItemBase> items, bool showHeader = true)
    {
        return new ConnectionGroup(_localizer, _mainWindowOverlayActivator, groupType, items, showHeader);
    }
}