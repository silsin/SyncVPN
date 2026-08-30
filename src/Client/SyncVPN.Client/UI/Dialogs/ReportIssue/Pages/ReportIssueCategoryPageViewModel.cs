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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SyncVPN.Api.Contracts.ReportAnIssue;
using SyncVPN.Client.Contracts.Services.Browsing;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Messages;
using SyncVPN.Client.Core.Models.ReportIssue;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Feedback.Contracts;
using SyncVPN.Client.Mappers;
using SyncVPN.Client.UI.Dialogs.ReportIssue.Bases;

namespace SyncVPN.Client.UI.Dialogs.ReportIssue.Pages;

public partial class ReportIssueCategoryPageViewModel : ReportIssuePageViewModelBase
{
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IReportIssueDataProvider _dataProvider;
    private readonly IUrlsBrowser _urlsBrowser;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Suggestions))]
    private IssueCategory? _category;

    public List<IssueSuggestion> Suggestions => Category?.Suggestions ?? [];

    public override string Title => Category?.Name ?? string.Empty;

    public ReportIssueCategoryPageViewModel(
        IEventMessageSender eventMessageSender,
        IReportIssueDataProvider dataProvider,
        IUrlsBrowser urlsBrowser,
        IReportIssueViewNavigator parentViewNavigator,
        IViewModelHelper viewModelHelper)
        : base(parentViewNavigator, viewModelHelper)
    {
        _eventMessageSender = eventMessageSender;
        _dataProvider = dataProvider;
        _urlsBrowser = urlsBrowser;
    }

    public override void OnNavigatedTo(object parameter, bool isBackNavigation)
    {
        base.OnNavigatedTo(parameter, isBackNavigation);

        Category = parameter as IssueCategory;
    }

    [RelayCommand]
    public async Task GoToContactFormAsync()
    {
        if (Category is not null)
        {
            await ParentViewNavigator.NavigateToContactViewAsync(Category);
        }
    }

    [RelayCommand]
    public void BrowseLink(string parameter)
    {
        if (!string.IsNullOrEmpty(parameter))
        {
            _urlsBrowser.BrowseTo(parameter);
        }
    }

    protected override async void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        await InvalidateCategoryAsync();
    }

    private async Task InvalidateCategoryAsync()
    {
        if (Category == null)
        {
            return;
        }

        List<IssueCategoryResponse> categories = await _dataProvider.GetCategoriesAsync();

        Category = ReportIssueMapper.Map(categories.First(c => c.SubmitLabel == Category.Key));
    }

    partial void OnCategoryChanged(IssueCategory? value)
    {
        _eventMessageSender.Send(new ReportIssueCategoryChangedMessage());
    }
}