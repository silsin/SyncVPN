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

using SyncVPN.Api.Contracts;
using SyncVPN.Api.Contracts.ReportAnIssue;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Contracts.Messages;
using SyncVPN.Client.Logic.Feedback.Contracts;
using SyncVPN.Client.Logic.Feedback.Models;

namespace SyncVPN.Client.Logic.Feedback;

public class ReportIssueDataProvider : IReportIssueDataProvider, IEventMessageReceiver<LanguageChangedMessage>
{
    private readonly IApiClient _apiClient;
    private readonly List<string> _validInputTypes = new() { InputType.SingleLineInput, InputType.MultiLineInput };
    private List<IssueCategoryResponse> _categories = new();

    public ReportIssueDataProvider(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<IssueCategoryResponse>> GetCategoriesAsync()
    {
        if (!_categories.Any())
        {
            await FetchDataAsync();
        }

        return _categories.ToList();
    }

    public void Receive(LanguageChangedMessage message)
    {
        // Clear categories to force fetching them again next time
        _categories.Clear();
    }

    private async Task FetchDataAsync()
    {
        try
        {
            ApiResponseResult<ReportAnIssueFormResponse> response = await _apiClient.GetReportAnIssueFormData();

            if (!response.Success)
            {
                throw new HttpRequestException("Error trying to retrieve Report an issue form data from the API");
            }

            if (!IsDataValid(response.Value.Categories))
            {
                throw new InvalidDataException("Invalid data received from the API for Report an issue form data ");
            }

            _categories = response.Value.Categories;
        }
        catch
        {
            _categories = ReportIssueDefaultDataProvider.GetCategories();
        }
    }

    private bool IsDataValid(IList<IssueCategoryResponse> categories)
    {
        return categories.All(category =>
            !string.IsNullOrEmpty(category.Label) &&
            !string.IsNullOrEmpty(category.SubmitLabel) &&
            category.InputFields != null &&
            !category.InputFields.Any(
                input => string.IsNullOrEmpty(input.Label) ||
                         string.IsNullOrEmpty(input.SubmitLabel) ||
                         !_validInputTypes.Contains(input.Type)));
    }
}