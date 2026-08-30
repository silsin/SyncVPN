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

using CommunityToolkit.Mvvm.Input;
using SyncVPN.Api.Contracts.ReportAnIssue;
using SyncVPN.Client.Common.Collections;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Models.ReportIssue;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Logic.Feedback.Contracts;
using SyncVPN.Client.Mappers;
using SyncVPN.Client.UI.Dialogs.ReportIssue.Bases;

namespace SyncVPN.Client.UI.Dialogs.ReportIssue.Pages;

public partial class ReportIssueCategoriesPageViewModel : ReportIssuePageViewModelBase
{
    private readonly IReportIssueDataProvider _dataProvider;

    private SemaphoreSlim _semaphore = new(1);

    public SmartObservableCollection<IssueCategory> Categories { get; }

    public ReportIssueCategoriesPageViewModel(
        IReportIssueDataProvider dataProvider,
        IReportIssueViewNavigator parentViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, viewModelHelper)
    {
        _dataProvider = dataProvider;

        Categories = [];
    }

    public override async void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        await InvalidateCategoriesAsync();
    }

    [RelayCommand]
    public async Task SelectCategoryAsync(IssueCategory category)
    {
        await ParentViewNavigator.NavigateToCategoryViewAsync(category);
    }

    protected override async void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        await InvalidateCategoriesAsync();
    }

    private async Task InvalidateCategoriesAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            List<IssueCategoryResponse> categories = await _dataProvider.GetCategoriesAsync();

            Categories.Reset(categories.Select(ReportIssueMapper.Map));
        }
        finally
        {
            _semaphore.Release();
        }
    }
}