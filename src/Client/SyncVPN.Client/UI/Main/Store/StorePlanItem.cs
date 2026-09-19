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

using System.Globalization;
using SyncVPN.Api.V2.Contracts.Plans;
using SyncVPN.Client.Core.Bases.Models;
using SyncVPN.Client.Localization.Contracts;

namespace SyncVPN.Client.UI.Main.Store;

// GET /plans lists a price per currency code with no locale hint from the backend - this picks the
// current OS region's currency when the plan offers it, falling back to USD and then to whatever
// currency the plan does list, rather than requiring a currency-selection step in the UI.
public class StorePlanItem : ModelBase
{
    private static readonly Dictionary<string, string> CurrencySymbols = new(StringComparer.OrdinalIgnoreCase)
    {
        ["usd"] = "$",
        ["eur"] = "€",
        ["gbp"] = "£",
    };

    private readonly int _months;
    private readonly int _users;

    public long PlanId { get; }

    public string Name { get; }

    public bool IsCurrentPlan { get; }

    // GET /plans has no "recommended" flag - the longest-billing-cycle plan is marked recommended as
    // the best-value heuristic common to VPN pricing pages, computed by the caller across the whole list.
    public bool IsRecommended { get; }

    public string PriceDisplay { get; }

    // Upper-cased ISO code of the currency actually shown in PriceDisplay (e.g. "EUR") - passed as-is
    // to POST /checkout-links so the checkout page bills in the same currency the user was quoted.
    public string CurrencyCode { get; }

    public string BillingCycleDisplay => FormatCycle(_months);

    public bool HasDevicesInfo => _users > 0;

    public string DevicesText => Localizer.GetPluralFormat("Store_Plan_Devices", _users);

    public string CurrentPlanBadgeText => Localizer.Get("Store_Plan_CurrentPlanBadge");

    public string RecommendedBadgeText => Localizer.Get("Store_Plan_RecommendedBadge");

    public string UpgradeButtonText => Localizer.Get("Store_Plan_UpgradeButton");

    public StorePlanItem(Plan plan, bool isCurrentPlan, bool isRecommended, ILocalizationProvider localizer)
        : base(localizer)
    {
        PlanId = plan.Id;
        Name = plan.Name;
        IsCurrentPlan = isCurrentPlan;
        IsRecommended = isRecommended;
        _months = plan.Months;
        _users = plan.Users;

        (string currencyCode, string amount) = SelectPrice(plan.Prices);
        CurrencyCode = currencyCode.ToUpperInvariant();
        PriceDisplay = FormatPrice(currencyCode, amount);
    }

    public void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(BillingCycleDisplay));
        OnPropertyChanged(nameof(DevicesText));
        OnPropertyChanged(nameof(CurrentPlanBadgeText));
        OnPropertyChanged(nameof(RecommendedBadgeText));
        OnPropertyChanged(nameof(UpgradeButtonText));
    }

    private static (string CurrencyCode, string Amount) SelectPrice(Dictionary<string, string> prices)
    {
        if (prices.Count == 0)
        {
            return (string.Empty, string.Empty);
        }

        string preferredCurrency = RegionInfo.CurrentRegion.ISOCurrencySymbol.ToLowerInvariant();

        return prices.TryGetValue(preferredCurrency, out string? preferredAmount)
            ? (preferredCurrency, preferredAmount)
            : prices.TryGetValue("usd", out string? usdAmount)
                ? ("usd", usdAmount)
                : (prices.First().Key, prices.First().Value);
    }

    private static string FormatPrice(string currencyCode, string amount)
    {
        if (currencyCode.Length == 0)
        {
            return string.Empty;
        }

        string formattedAmount = decimal.TryParse(amount, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value)
            ? value.ToString("0.00", CultureInfo.InvariantCulture)
            : amount;

        return CurrencySymbols.TryGetValue(currencyCode, out string? symbol)
            ? $"{symbol}{formattedAmount}"
            : $"{formattedAmount} {currencyCode.ToUpperInvariant()}";
    }

    private string FormatCycle(int months)
    {
        return months switch
        {
            1 => Localizer.Get("Store_Plan_Cycle_Monthly"),
            12 => Localizer.Get("Store_Plan_Cycle_Yearly"),
            _ => Localizer.GetPluralFormat("Store_Plan_Cycle_NMonths", months),
        };
    }
}
