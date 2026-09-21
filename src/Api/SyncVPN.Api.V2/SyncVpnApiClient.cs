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

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts;
using SyncVPN.Api.V2.Contracts.Account;
using SyncVPN.Api.V2.Contracts.Auth;
using SyncVPN.Api.V2.Contracts.Billing;
using SyncVPN.Api.V2.Contracts.CheckoutLinks;
using SyncVPN.Api.V2.Contracts.Devices;
using SyncVPN.Api.V2.Contracts.Dns;
using SyncVPN.Api.V2.Contracts.Plans;
using SyncVPN.Api.V2.Contracts.Purchases;
using SyncVPN.Api.V2.Contracts.Servers;
using SyncVPN.Api.V2.Contracts.Transactions;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.ApiLogs;

namespace SyncVPN.Api.V2;

// Talks to the new SyncVPN-owned backend (https://syncvpn.com/api). Deliberately independent of
// BaseApiClient/ApiClient (legacy backend) - no SRP, no x-pm-* headers, no retry/human-verification/alt-routing
// pipeline. Those concerns can be layered back in per-endpoint if a migrated capability turns out to need
// them, rather than inherited wholesale from a pipeline built for a different backend.
public class SyncVpnApiClient : ISyncVpnApiClient, IDisposable
{
    private const string AppTokenHeaderName = "token";
    private const string DeviceIdHeaderName = "Deviceid";

    private readonly HttpClient _httpClient;
    private readonly ISettings _settings;
    private readonly ILogger _logger;

    public SyncVpnApiClient(ISyncVpnApiHostProvider hostProvider, ISyncVpnAppTokenProvider appTokenProvider, ISettings settings, ILogger logger)
    {
        _httpClient = new HttpClient { BaseAddress = hostProvider.GetBaseUri() };
        _httpClient.DefaultRequestHeaders.Add(AppTokenHeaderName, appTokenProvider.GetAppToken());
        // Without this, some error responses (e.g. an invalid app token) fall back to an HTML page
        // instead of JSON - see SyncVpnApiClient.GetWebLoginStatusAsync's 404 case.
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _settings = settings;
        _logger = logger;
    }

    public async Task<ApiResponseResult<RegisterDeviceResponse>> RegisterDeviceAsync(RegisterDeviceRequest request, CancellationToken cancellationToken = default)
    {
        // The one endpoint exempt from the Deviceid header - it's how the device gets one in the first place.
        using HttpRequestMessage httpRequest = new(HttpMethod.Post, "devices/register")
        {
            Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")
        };
        using HttpResponseMessage response = await SendAsync(httpRequest, cancellationToken);
        return await ReadResponseAsync<RegisterDeviceResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<CountryListResponse>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "countries");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<CountryListResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<BillingCountryListResponse>> GetBillingCountriesAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "billing/countries");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<BillingCountryListResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<ServerListResponse>> GetServersAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "servers");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<ServerListResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<ServerListResponse>> GetProServersAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "servers/pro");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<ServerListResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<FavoriteServersResponse>> GetFavoriteServersAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "servers/favorites");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<FavoriteServersResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<FavoriteServerActionResponse>> AddFavoriteServerAsync(long serverId, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, $"servers/{serverId}/favorite");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<FavoriteServerActionResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<RemoveFavoriteServerResponse>> RemoveFavoriteServerAsync(long serverId, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Delete, $"servers/{serverId}/favorite");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<RemoveFavoriteServerResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<RateServerResponse>> RateServerAsync(long serverId, int rate, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, $"servers/{serverId}/rate");
        request.Content = new StringContent(JsonConvert.SerializeObject(new RateServerRequest { Rate = rate }), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<RateServerResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<ClaimAccountResponse>> ClaimAccountAsync(ClaimAccountRequest request, CancellationToken cancellationToken = default)
    {
        return await PostClaimAccountAsync("account", request, cancellationToken);
    }

    public async Task<ApiResponseResult<ClaimAccountResponse>> ClaimAccountByCityAsync(ClaimAccountByCityRequest request, CancellationToken cancellationToken = default)
    {
        return await PostClaimAccountAsync("account/city", request, cancellationToken);
    }

    private async Task<ApiResponseResult<ClaimAccountResponse>> PostClaimAccountAsync(string path, object requestBody, CancellationToken cancellationToken)
    {
        using HttpRequestMessage httpRequest = CreateRequest(HttpMethod.Post, path);
        httpRequest.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await SendAsync(httpRequest, cancellationToken);
        return await ReadResponseAsync<ClaimAccountResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<UsageReportResponse>> ReportUsageAsync(UsageReportRequest request, CancellationToken cancellationToken = default)
    {
        // Free accounts don't need a Bearer for this call, but a logged-in Pro device must send one -
        // CreateRequest attaches it automatically whenever one is stored; the backend enforces the Pro requirement.
        using HttpRequestMessage httpRequest = CreateRequest(HttpMethod.Post, "account/usage");
        httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await SendAsync(httpRequest, cancellationToken);
        return await ReadResponseAsync<UsageReportResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<AccountDnsFilterResponse>> GetDnsFiltersAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "account/dns-filters");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<AccountDnsFilterResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<UpdateAccountDnsFiltersResponse>> UpdateDnsFiltersAsync(DnsFilterPatch patch, CancellationToken cancellationToken = default)
    {
        // Pro-gated: requires Authorization: Bearer <DeviceToken> in addition to Deviceid, attached
        // automatically by CreateRequest. A logged-out device (no SyncVpnDeviceToken) will 401 - callers
        // should surface that as "not logged in" rather than a generic failure.
        UpdateAccountDnsFiltersRequest requestBody = new() { Filters = patch };
        using HttpRequestMessage request = CreateRequest(HttpMethod.Patch, "account/dns-filters");
        request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<UpdateAccountDnsFiltersResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<PlanListResponse>> GetPlansAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "plans");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<PlanListResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<PurchaseResponse>> SubmitPurchaseAsync(PurchaseRequest request, CancellationToken cancellationToken = default)
    {
        // Guest purchases don't require a device token, but a logged-in device should still send its
        // bearer if it has one so the backend can attach the purchase to that user - CreateRequest
        // attaches it automatically whenever one is stored, rather than requiring it.
        using HttpRequestMessage httpRequest = CreateRequest(HttpMethod.Post, "purchases");
        httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await SendAsync(httpRequest, cancellationToken);
        return await ReadResponseAsync<PurchaseResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<CheckoutLinkResponse>> CreateCheckoutLinkAsync(CheckoutLinkRequest request, CancellationToken cancellationToken = default)
    {
        // Guests can request action=purchase without a Bearer; action=renew requires one, attached
        // automatically by CreateRequest whenever a device token is stored.
        using HttpRequestMessage httpRequest = CreateRequest(HttpMethod.Post, "checkout-links");
        httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await SendAsync(httpRequest, cancellationToken);
        return await ReadResponseAsync<CheckoutLinkResponse>(response, cancellationToken);
    }

    // poll_token travels in the JSON body (not a header or query string) so it never lands in an
    // access log - same reasoning as GetWebLoginStatusAsync.
    public async Task<ApiResponseResult<CheckoutStatusResponse>> GetCheckoutLinkStatusAsync(CheckoutStatusRequest request, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage httpRequest = CreateRequest(HttpMethod.Post, "checkout-links/status");
        httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await SendAsync(httpRequest, cancellationToken);
        return await ReadResponseAsync<CheckoutStatusResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<TransactionListResponse>> GetTransactionsAsync(int page = 1, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, $"transactions?page={page}");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<TransactionListResponse>(response, cancellationToken);
    }

    public Task<ApiResponseResult<LoginAttemptResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        return PostLoginAttemptAsync("auth/login", request, cancellationToken);
    }

    public Task<ApiResponseResult<LoginAttemptResponse>> CodeLoginAsync(CodeLoginRequest request, CancellationToken cancellationToken = default)
    {
        return PostLoginAttemptAsync("auth/code-login", request, cancellationToken);
    }

    private async Task<ApiResponseResult<LoginAttemptResponse>> PostLoginAttemptAsync(string path, object requestBody, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, path);
        request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        LogResponse(response, body);

        if (response.StatusCode == HttpStatusCode.Accepted)
        {
            TwoFactorChallengeResponse? challenge = JsonConvert.DeserializeObject<TwoFactorChallengeResponse>(body);
            return ApiResponseResult<LoginAttemptResponse>.Ok(response, new LoginAttemptResponse { RequiresTwoFactor = true, Challenge = challenge });
        }

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponseResult<LoginAttemptResponse>.Fail(response, body);
        }

        LoginResponse? login = JsonConvert.DeserializeObject<LoginResponse>(body);
        return ApiResponseResult<LoginAttemptResponse>.Ok(response, new LoginAttemptResponse { RequiresTwoFactor = false, Login = login });
    }

    // Starts a browser-based login attempt for this (already-registered) device - the Deviceid header
    // is attached automatically by CreateRequest from _settings.SyncVpnDeviceId. No request body.
    public async Task<ApiResponseResult<WebAppLoginResponse>> StartWebLoginAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, "auth/web-app");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<WebAppLoginResponse>(response, cancellationToken);
    }

    // poll_token travels in the JSON body (not a header or query string) so it never lands in an
    // access log - see WebAppLoginData.PollToken.
    public async Task<ApiResponseResult<WebAppLoginStatusResponse>> GetWebLoginStatusAsync(string key, string pollToken, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, "auth/web-app/status");
        WebAppLoginStatusRequest body = new() { Key = key, PollToken = pollToken };
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<WebAppLoginStatusResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<LoginResponse>> VerifyTwoFactorAsync(TwoFactorVerifyRequest request, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage httpRequest = CreateRequest(HttpMethod.Post, "auth/2fa/verify");
        httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await SendAsync(httpRequest, cancellationToken);
        return await ReadResponseAsync<LoginResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<AuthenticatedDeviceResponse>> GetAuthenticatedDeviceAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "auth/me");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<AuthenticatedDeviceResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<LogoutResponse>> LogoutAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, "auth/logout");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken);
        return await ReadResponseAsync<LogoutResponse>(response, cancellationToken);
    }

    // Every call through here (i.e. every endpoint except RegisterDeviceAsync, which predates having a
    // Deviceid at all) automatically carries the Bearer DeviceToken whenever one is stored, regardless of
    // whether that particular endpoint strictly requires it - a device that's logged in should identify
    // itself on every request, not just the ones an individual call site remembered to opt into. Pro-gated
    // endpoints (e.g. PATCH account/dns-filters) still simply 401 on their own if the token turns out to
    // be missing or invalid; this just guarantees it's never missing purely because a call site forgot to ask.
    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        HttpRequestMessage request = new(method, path);

        if (_settings.SyncVpnDeviceId is { Length: > 0 } deviceId)
        {
            request.Headers.Add(DeviceIdHeaderName, deviceId);
        }

        if (_settings.SyncVpnDeviceToken is { Length: > 0 } deviceToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deviceToken);
        }

        return request;
    }

    private async Task<ApiResponseResult<T>> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        LogResponse(response, body);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponseResult<T>.Fail(response, body);
        }

        T? value = JsonConvert.DeserializeObject<T>(body);
        return ApiResponseResult<T>.Ok(response, value);
    }

    // Every SendAsync call site routes through here so every request/failure is visible in the log file
    // regardless of build configuration (unlike LoggingHandler's legacy backend pipeline, which strips its
    // header/body logging out of Release builds) - this client talks to a newer, less-proven backend
    // where seeing exactly what was sent and got back is the main way to diagnose "data not coming back".
    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string description = DescribeRequest(request);
        _logger.Info<ApiRequestLog>(description);

        try
        {
            return await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.Error<ApiErrorLog>($"{description} failed: {ex.CombinedMessage()}");
            throw;
        }
    }

    // Response body is logged in full (unlike LoggingHandler, which only logs status/headers) - every
    // SyncVpnApiClient response is a small JSON payload, never a large file download, so this is safe to
    // do unconditionally.
    private void LogResponse(HttpResponseMessage response, string body)
    {
        string description = DescribeRequest(response.RequestMessage);

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Accepted)
        {
            _logger.Info<ApiResponseLog>($"{description}: {(int)response.StatusCode} {response.StatusCode} - {body}");
        }
        else
        {
            _logger.Error<ApiErrorLog>($"{description}: {(int)response.StatusCode} {response.StatusCode} - {body}");
        }
    }

    // request.RequestUri starts out relative (e.g. "account"), but HttpClient rewrites it in place to the
    // resolved absolute URI once the request has actually been sent - so by the time this runs against
    // response.RequestMessage, prepending BaseAddress again would duplicate it.
    private string DescribeRequest(HttpRequestMessage request)
    {
        if (request is null)
        {
            return string.Empty;
        }

        Uri uri = request.RequestUri.IsAbsoluteUri ? request.RequestUri : new Uri(_httpClient.BaseAddress, request.RequestUri);
        return $"{request.Method.Method} \"{uri}\"";
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
