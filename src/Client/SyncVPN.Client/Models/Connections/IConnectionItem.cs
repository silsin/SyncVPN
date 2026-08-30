/*
 * Copyright (c) 2024 Proton AG
 *
 * This file is part of SyncVPN.
 *
 * SyncVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * SyncVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General License for more details.
 *
 * You should have received a copy of the GNU General License
 * along with SyncVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using SyncVPN.Client.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Models;

namespace SyncVPN.Client.Models.Connections;

public interface IConnectionItem
{
    bool IsUnderMaintenance { get; }
    bool IsActiveConnection { get; }
    bool IsRestricted { get; }
    bool IsAvailable { get; }
    ConnectionGroupType GroupType { get; }
    string Header { get; }
    string Description { get; }
    bool IsDescriptionVisible { get; }
    string? ToolTip { get; }
    bool IsCounted { get; }
    object FirstSortProperty { get; }
    object SecondSortProperty { get; }
    string PrimaryActionLabel { get; }
    string PrimaryCommandAutomationId { get; }
    string SecondaryCommandAutomationId { get; }
    string ActiveConnectionAutomationId { get; }

    void InvalidateIsActiveConnection(ConnectionDetails? currentConnectionDetails);
    void InvalidateIsRestricted(bool isPaidUser);
}
