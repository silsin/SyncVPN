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

using SyncVPN.Api.V2.Contracts.Common;
using SyncVPN.Client.Localization.Contracts;

namespace SyncVPN.Client.Extensions;

// SyncVpnApiClient failures carry the backend's raw response body as their error string (callers like
// PurchaseService/CheckoutLinkService parse it to branch on error_code, so it can't be rewritten at
// the source). Anything shown to the user must go through here instead of displaying that string
// directly - otherwise raw JSON like {"message":"Too Many Attempts."} or an HTML error page ends up
// in the UI.
public static class UserFacingErrorExtensions
{
    public static string GetUserFacingError(this ILocalizationProvider localizer, string? rawError)
    {
        if (string.IsNullOrWhiteSpace(rawError))
        {
            return localizer.Get("Common_Error_Generic");
        }

        string error = rawError.Trim();

        if (IsTooManyAttempts(error))
        {
            return localizer.Get("Common_Error_TooManyAttempts");
        }

        if (error.StartsWith('{') || error.StartsWith('['))
        {
            return GetMessageFromJson(localizer, error);
        }

        // HTML error pages (e.g. a 404 from a proxy) and technical transport failures are never
        // meaningful to the user.
        if (error.StartsWith('<'))
        {
            return localizer.Get("Common_Error_Generic");
        }

        if (IsConnectivityError(error))
        {
            return localizer.Get("Common_Error_NoConnection");
        }

        // Plain text that isn't JSON/HTML was written by the app itself for display (e.g. "Incorrect
        // login credentials. Please try again").
        return error;
    }

    private static string GetMessageFromJson(ILocalizationProvider localizer, string json)
    {
        SyncVpnErrorResponse? response = SyncVpnErrorResponse.TryParse(json);

        switch (response?.ErrorCode)
        {
            case SyncVpnErrorCodes.SystemMaintenance:
                return localizer.Get("Common_Error_Maintenance");
            case SyncVpnErrorCodes.UserBlocked:
                return localizer.Get("Common_Error_Blocked");
        }

        if (response?.Blocked == true)
        {
            return localizer.Get("Common_Error_Blocked");
        }

        string? message = response?.Message?.Trim();
        if (string.IsNullOrEmpty(message) || IsTooManyAttempts(message))
        {
            return IsTooManyAttempts(message)
                ? localizer.Get("Common_Error_TooManyAttempts")
                : localizer.Get("Common_Error_Generic");
        }

        return message;
    }

    private static bool IsTooManyAttempts(string? error)
    {
        return error is not null
            && (error.Contains("too many attempts", StringComparison.OrdinalIgnoreCase)
                || error.Contains("too many requests", StringComparison.OrdinalIgnoreCase)
                || error.Contains("TOO_MANY", StringComparison.OrdinalIgnoreCase)
                || error.Contains("429", StringComparison.Ordinal));
    }

    private static bool IsConnectivityError(string error)
    {
        return error.Contains("No such host", StringComparison.OrdinalIgnoreCase)
            || error.Contains("connection attempt failed", StringComparison.OrdinalIgnoreCase)
            || error.Contains("actively refused", StringComparison.OrdinalIgnoreCase)
            || error.Contains("timed out", StringComparison.OrdinalIgnoreCase)
            || error.Contains("SSL connection", StringComparison.OrdinalIgnoreCase);
    }
}
