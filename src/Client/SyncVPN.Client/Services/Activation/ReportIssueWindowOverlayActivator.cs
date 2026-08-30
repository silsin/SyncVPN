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

using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Activation.Bases;
using SyncVPN.Client.Core.Services.Mapping;
using SyncVPN.Client.Core.Services.Selection;
using SyncVPN.Client.UI.Dialogs.ReportIssue;

namespace SyncVPN.Client.Services.Activation;

public class ReportIssueWindowOverlayActivator : OverlayActivatorBase<ReportIssueWindow>, IReportIssueWindowOverlayActivator
{
    public ReportIssueWindowOverlayActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationService localizationService,
        IOverlayViewMapper overlayViewMapper)
        : base(logger, 
               uiThreadDispatcher, 
               themeSelector, 
               settings, 
               localizationService, 
               overlayViewMapper)
    { }
}