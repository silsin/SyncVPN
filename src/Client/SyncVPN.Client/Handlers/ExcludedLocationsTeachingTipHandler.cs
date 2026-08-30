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

using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Core.Messages;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Core.Services.TeachingTips;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Handlers.Bases;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.StatisticalEvents.Contracts;

namespace SyncVPN.Client.Handlers;

public class ExcludedLocationsTeachingTipHandler : IHandler,
    IEventMessageReceiver<HomePageDisplayedAfterLoginMessage>
{
    private static readonly TimeSpan _teachingTipDelay = TimeSpan.FromSeconds(1);

    private readonly ISettings _settings;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly ITeachingTipService _teachingTipService;
    private readonly IMainViewNavigator _mainViewNavigator;
    private readonly ISettingsViewNavigator _settingsViewNavigator;
    private readonly IProductPromptDisplayReporter _productPromptDisplayReporter;
    private readonly IProductPromptActionReporter _productPromptActionReporter;

    public ExcludedLocationsTeachingTipHandler(
        ISettings settings,
        IUIThreadDispatcher uiThreadDispatcher,
        ITeachingTipService teachingTipService,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IProductPromptDisplayReporter productPromptDisplayReporter,
        IProductPromptActionReporter productPromptActionReporter)
    {
        _settings = settings;
        _uiThreadDispatcher = uiThreadDispatcher;
        _teachingTipService = teachingTipService;
        _mainViewNavigator = mainViewNavigator;
        _settingsViewNavigator = settingsViewNavigator;
        _productPromptDisplayReporter = productPromptDisplayReporter;
        _productPromptActionReporter = productPromptActionReporter;
    }

    public void Receive(HomePageDisplayedAfterLoginMessage message)
    {
        if (!ShouldShowTeachingTip())
        {
            return;
        }

        _uiThreadDispatcher.TryEnqueue(async () =>
        {
            await Task.Delay(_teachingTipDelay);
            if (_teachingTipService.TryShow(TeachingTipKey.ExcludedLocations, OnTeachingTipInvoked, OnTeachingTipDismissed))
            {
                _settings.WasExcludedLocationsTeachingTipDisplayed = true;
                _productPromptDisplayReporter.Report(PromptType.FeatureDiscovery, PromptContext.ConnectionPreferencesTooltip);
            }
        });
    }

    private bool ShouldShowTeachingTip()
    {
        return _settings is
        {
            WasExcludedLocationsTeachingTipDisplayed: false,
            VpnPlan.IsPaid: true,
            WasWelcomeOverlayDisplayed: true,
        };
    }

    private void OnTeachingTipInvoked()
    {
        _productPromptActionReporter.Report(PromptType.FeatureDiscovery, PromptContext.ConnectionPreferencesTooltip, PromptAction.Configure);
        NavigateToConnectionPreferencesAsync().FireAndForget();
    }

    private void OnTeachingTipDismissed()
    {
        _productPromptActionReporter.Report(PromptType.FeatureDiscovery, PromptContext.ConnectionPreferencesTooltip, PromptAction.Dismiss);
    }

    private async Task NavigateToConnectionPreferencesAsync()
    {
        await _mainViewNavigator.NavigateToSettingsViewAsync();
        await _settingsViewNavigator.NavigateToConnectionPreferencesSettingsViewAsync();
    }
}