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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts.Common;
using SyncVPN.Api.V2.Contracts.Plans;
using SyncVPN.Client.Common.Models;
using SyncVPN.Client.Contracts.Services.Browsing;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Logic.Auth.Contracts;
using SyncVPN.Client.Logic.Purchases.Contracts;
using SyncVPN.Client.Logic.Purchases.Contracts.Models;
using SyncVPN.Client.Logic.Users.Contracts;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.UI.Overlays.Store;

namespace SyncVPN.Client.UI.Main.Store;

// Plans/prices come from GET /plans (already wired end to end via IPlansProvider). Buying creates a
// one-time browser checkout link via POST /checkout-links (ICheckoutLinkService) and opens it - there
// is still no in-app payment collection, so this only ever needs the plan id, the currency already
// shown on the card, and (for a guest device with no account) an email collected just-in-time.
public partial class StorePageViewModel : PageViewModelBase<IMainViewNavigator>
{
    private readonly IPlansProvider _plansProvider;
    private readonly ISettings _settings;
    private readonly ICheckoutLinkService _checkoutLinkService;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IVpnPlanUpdater _vpnPlanUpdater;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IMainWindowOverlayActivator _overlayActivator;
    private readonly StoreGuestEmailOverlayViewModel _guestEmailOverlay;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasLoadError;

    [ObservableProperty]
    private string _loadErrorMessage = string.Empty;

    [ObservableProperty]
    private bool _isProcessingCheckout;

    public override string Title => Localizer.Get("Store_Page_Title");

    public string CurrentPlanLabel => _settings.VpnPlan.IsFreePlan || _settings.VpnPlan.IsDefaultPlan
        ? Localizer.Get("Account_VpnPlan_Free")
        : _settings.VpnPlan.Title;

    public ObservableCollection<StorePlanItem> Plans { get; } = [];

    public StorePageViewModel(
        IMainViewNavigator mainViewNavigator,
        IPlansProvider plansProvider,
        ISettings settings,
        ICheckoutLinkService checkoutLinkService,
        IUserAuthenticator userAuthenticator,
        IVpnPlanUpdater vpnPlanUpdater,
        IUrlsBrowser urlsBrowser,
        IMainWindowOverlayActivator overlayActivator,
        StoreGuestEmailOverlayViewModel guestEmailOverlay,
        IViewModelHelper viewModelHelper)
        : base(mainViewNavigator, viewModelHelper)
    {
        _plansProvider = plansProvider;
        _settings = settings;
        _checkoutLinkService = checkoutLinkService;
        _userAuthenticator = userAuthenticator;
        _vpnPlanUpdater = vpnPlanUpdater;
        _urlsBrowser = urlsBrowser;
        _overlayActivator = overlayActivator;
        _guestEmailOverlay = guestEmailOverlay;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        _ = LoadPlansAsync();
    }

    [RelayCommand]
    private Task RetryLoadPlansAsync()
    {
        return LoadPlansAsync();
    }

    [RelayCommand]
    private async Task UpgradeAsync(StorePlanItem plan)
    {
        string? guestEmail = null;

        if (!_userAuthenticator.IsLoggedIn)
        {
            guestEmail = await _guestEmailOverlay.RequestEmailAsync();
            if (guestEmail is null)
            {
                // User cancelled the email prompt.
                return;
            }
        }

        IsProcessingCheckout = true;

        CheckoutLinkResult result = await _checkoutLinkService.CreatePurchaseLinkAsync(plan.PlanId, plan.CurrencyCode, guestEmail);

        IsProcessingCheckout = false;

        if (result.Outcome != CheckoutLinkOutcome.Created || result.Data is null)
        {
            await _overlayActivator.ShowMessageAsync(new MessageDialogParameters
            {
                Title = Localizer.Get("Store_Checkout_ErrorTitle"),
                Message = BuildCheckoutErrorMessage(result),
                CloseButtonText = Localizer.Get("Common_Actions_Close"),
            });
            return;
        }

        _urlsBrowser.BrowseTo(result.Data.PaymentUrl);

        await WaitForCheckoutCompletionAsync(result.Data.Key, result.Data.PollToken, result.Data.PollIntervalSeconds);
    }

    // Races the "waiting for payment" dialog against the actual poll loop (ShowLoadingMessageAsync
    // always returns as soon as either finishes) so the user can cancel out of waiting at any time -
    // if they do, the poll loop is cancelled too rather than left running unattended in the background.
    private async Task WaitForCheckoutCompletionAsync(string key, string pollToken, int pollIntervalSeconds)
    {
        using CancellationTokenSource pollCts = new();

        Task<CheckoutCompletionResult> waitTask = _checkoutLinkService.WaitForCompletionAsync(key, pollToken, pollIntervalSeconds, pollCts.Token);

        await _overlayActivator.ShowLoadingMessageAsync(
            new MessageDialogParameters
            {
                Title = Localizer.Get("Store_Checkout_WaitingTitle"),
                Message = Localizer.Get("Store_Checkout_WaitingMessage"),
                ShowLoadingAnimation = true,
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
            },
            waitTask);

        if (!waitTask.IsCompleted)
        {
            pollCts.Cancel();

            try
            {
                await waitTask;
            }
            catch (OperationCanceledException)
            {
                // Expected - the user closed the waiting dialog before a terminal state was reached.
            }

            return;
        }

        await HandleCheckoutCompletionAsync(await waitTask);
    }

    private async Task HandleCheckoutCompletionAsync(CheckoutCompletionResult completion)
    {
        // Completed and LoginFailed both mean the payment itself succeeded - LoginFailed just means the
        // automatic sign-in exchange didn't, so it still gets the success framing, not the error one.
        bool paymentSucceeded = completion.Outcome is CheckoutCompletionOutcome.Completed or CheckoutCompletionOutcome.LoginFailed;

        await _overlayActivator.ShowMessageAsync(new MessageDialogParameters
        {
            Title = paymentSucceeded ? Localizer.Get("Store_Checkout_SuccessTitle") : Localizer.Get("Store_Checkout_ErrorTitle"),
            Message = completion.Outcome == CheckoutCompletionOutcome.Completed
                ? Localizer.Get("Store_Checkout_SuccessMessage")
                : BuildCheckoutCompletionErrorMessage(completion),
            CloseButtonText = Localizer.Get("Common_Actions_Close"),
        });

        if (paymentSucceeded)
        {
            // Nothing else in this flow tells the rest of the app the plan changed - without this, the
            // sidebar's plan badge (and this page's own "your plan"/"recommended" badges, which also key
            // off ISettings.VpnPlan) would keep showing Free/the old plan until some unrelated trigger
            // (e.g. reconnecting, or the app regaining window focus) happened to refresh it.
            await _vpnPlanUpdater.ForceUpdateAsync();
            await LoadPlansAsync();
        }
    }

    private string BuildCheckoutErrorMessage(CheckoutLinkResult result)
    {
        return !string.IsNullOrWhiteSpace(result.Error?.Message)
            ? result.Error.Message
            : Localizer.Get("Store_Checkout_ErrorMessage");
    }

    private string BuildCheckoutCompletionErrorMessage(CheckoutCompletionResult completion)
    {
        if (!string.IsNullOrWhiteSpace(completion.Error?.Message))
        {
            return completion.Error.Message;
        }

        return completion.Outcome switch
        {
            CheckoutCompletionOutcome.LoginFailed => Localizer.Get("Store_Checkout_LoginFailedMessage"),
            CheckoutCompletionOutcome.Expired => Localizer.Get("Store_Checkout_ExpiredMessage"),
            CheckoutCompletionOutcome.Failed => Localizer.Get("Store_Checkout_FailedMessage"),
            CheckoutCompletionOutcome.Refunded => Localizer.Get("Store_Checkout_RefundedMessage"),
            _ => Localizer.Get("Store_Checkout_ErrorMessage"),
        };
    }

    private async Task LoadPlansAsync()
    {
        IsLoading = true;
        HasLoadError = false;

        ApiResponseResult<PlanListResponse> response = await _plansProvider.GetPlansAsync();

        Plans.Clear();

        if (!response.Success || response.Value is null)
        {
            HasLoadError = true;
            LoadErrorMessage = BuildErrorMessage(response.Error);
            IsLoading = false;
            return;
        }

        List<Plan> plans = response.Value.Data;
        long recommendedPlanId = plans.Count > 1
            ? plans.OrderByDescending(plan => plan.Months).First().Id
            : -1;

        foreach (Plan plan in plans)
        {
            Plans.Add(new StorePlanItem(plan, IsCurrentPlan(plan), plan.Id == recommendedPlanId, Localizer));
        }

        IsLoading = false;
    }

    // GET /plans returns a structured body on failure (404 invalid token, 422 device not registered,
    // 503 maintenance) - surface the server's own message when present instead of one generic string,
    // same parsing PurchaseService already relies on for /purchases failures.
    private string BuildErrorMessage(string? rawError)
    {
        SyncVpnErrorResponse? error = SyncVpnErrorResponse.TryParse(rawError);

        return !string.IsNullOrWhiteSpace(error?.Message)
            ? error.Message
            : Localizer.Get("Store_LoadError_Message");
    }

    private bool IsCurrentPlan(Plan plan)
    {
        return !_settings.VpnPlan.IsDefaultPlan
            && !_settings.VpnPlan.IsFreePlan
            && string.Equals(plan.Name, _settings.VpnPlan.Name, StringComparison.OrdinalIgnoreCase);
    }
}
