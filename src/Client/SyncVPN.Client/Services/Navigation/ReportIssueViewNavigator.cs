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

using SyncVPN.Logging.Contracts;
using SyncVPN.Client.Core.Services.Mapping;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Core.Services.Navigation.Bases;
using SyncVPN.Client.UI.Dialogs.ReportIssue.Pages;
using SyncVPN.Client.Core.Models.ReportIssue;
using SyncVPN.Client.Core.Enums;
using SyncVPN.Client.Common.Dispatching;

namespace SyncVPN.Client.Services.Navigation;

public class ReportIssueViewNavigator : ViewNavigatorBase, IReportIssueViewNavigator
{
    public override bool IsNavigationStackEnabled => true;

    public override FrameLoadedBehavior LoadBehavior { get; protected set; } = FrameLoadedBehavior.NavigateToDefaultViewIfEmpty;

    public ReportIssueViewNavigator(
        ILogger logger,
        IPageViewMapper pageViewMapper,
        IUIThreadDispatcher uiThreadDispatcher)
        : base(logger, pageViewMapper, uiThreadDispatcher)
    { }

    public Task<bool> NavigateToCategoriesViewAsync()
    {
        return NavigateToAsync<ReportIssueCategoriesPageViewModel>();
    }

    public async Task<bool> NavigateToCategoryViewAsync(IssueCategory category)
    {
        bool navigated = category.Suggestions.Any()
            ? await NavigateToAsync<ReportIssueCategoryPageViewModel>(category)
            : await NavigateToContactViewAsync(category);

        // Clear back stack so the back button brings to the category selection page
        ClearBackStack();

        return navigated;
    }

    public Task<bool> NavigateToContactViewAsync(IssueCategory category)
    {
        return NavigateToAsync<ReportIssueContactPageViewModel>(category);
    }

    public Task<bool> NavigateToResultViewAsync(bool isReportSent)
    {
        return NavigateToAsync<ReportIssueResultPageViewModel>(isReportSent);
    }

    public override Task<bool> NavigateToDefaultAsync()
    {
        return NavigateToCategoriesViewAsync();
    }
}