/*
 * Copyright (c) 2024 Proton AG
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
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Activation.Bases;
using SyncVPN.Client.Core.Services.Selection;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Dialogs.Troubleshooting;
using SyncVPN.Logging.Contracts;

namespace SyncVPN.Client.Services.Activation;

public class TroubleshootingWindowActivator : DialogActivatorBase<TroubleshootingWindow>, ITroubleshootingWindowActivator
{
    public override string WindowTitle => Localizer.Get("Dialogs_Troubleshooting_Title");

    public TroubleshootingWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationService localizationService,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IMainWindowActivator mainWindowActivator)
        : base(logger,
               uiThreadDispatcher,
               themeSelector,
               settings,
               localizationService,
               localizer,
               iconSelector,
               mainWindowActivator)
    { }
}