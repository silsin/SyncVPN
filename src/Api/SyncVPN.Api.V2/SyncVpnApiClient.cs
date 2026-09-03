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
using SyncVPN.Api.V2.Contracts.Devices;
using SyncVPN.Api.V2.Contracts.Dns;
using SyncVPN.Api.V2.Contracts.Plans;
using SyncVPN.Api.V2.Contracts.Purchases;
using SyncVPN.Api.V2.Contracts.Servers;
using SyncVPN.Api.V2.Contracts.Transactions;
using SyncVPN.Client.Settings.Contracts;

namespace SyncVPN.Api.V2;

// Talks to the new SyncVPN-owned backend (https://syncvpn.com/api). Deliberately independent of
// BaseApiClient/ApiClient (Proton) - no SRP, no x-pm-* headers, no retry/human-verification/alt-routing
// pipeline. Those concerns can be layered back in per-endpoint if a migrated capability turns out to need
// them, rather than inherited wholesale from a pipeline built for a different backend.
public class SyncVpnApiClient : ISyncVpnApiClient, IDisposable
{
    private const string AppTokenHeaderName = "token";
    private const string DeviceIdHeaderName = "Deviceid";

    private readonly HttpClient _httpClient;
    private readonly ISettings _settings;

    public SyncVpnApiClient(ISyncVpnApiHostProvider hostProvider, ISyncVpnAppTokenProvider appTokenProvider, ISettings settings)
    {
        _httpClient = new HttpClient { BaseAddress = hostProvider.GetBaseUri() };
        _httpClient.DefaultRequestHeaders.Add(AppTokenHeaderName, appTokenProvider.GetAppToken());
        _settings = settings;
    }

    public async Task<ApiResponseResult<RegisterDeviceResponse>> RegisterDeviceAsync(RegisterDeviceRequest request, CancellationToken cancellationToken = default)
    {
        // The one endpoint exempt from the Deviceid header - it's how the device gets one in the first place.
        using StringContent content = new(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await _httpClient.PostAsync("devices/register", content, cancellationToken);
        return await ReadResponseAsync<RegisterDeviceResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<CountryListResponse>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "countries");
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<CountryListResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<BillingCountryListResponse>> GetBillingCountriesAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "billing/countries");
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<BillingCountryListResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<ServerListResponse>> GetServersAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "servers");
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<ServerListResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<ServerListResponse>> GetProServersAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "servers/pro", requireDeviceToken: true);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<ServerListResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<FavoriteServersResponse>> GetFavoriteServersAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "servers/favorites", requireDeviceToken: true);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<FavoriteServersResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<FavoriteServerActionResponse>> AddFavoriteServerAsync(long serverId, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, $"servers/{serverId}/favorite", requireDeviceToken: true);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<FavoriteServerActionResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<RemoveFavoriteServerResponse>> RemoveFavoriteServerAsync(long serverId, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Delete, $"servers/{serverId}/favorite", requireDeviceToken: true);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<RemoveFavoriteServerResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<RateServerResponse>> RateServerAsync(long serverId, int rate, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, $"servers/{serverId}/rate", requireDeviceToken: true);
        request.Content = new StringContent(JsonConvert.SerializeObject(new RateServerRequest { Rate = rate }), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
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
        using HttpRequestMessage httpRequest = CreateRequest(HttpMethod.Post, path, requireDeviceToken: true);
        httpRequest.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        return await ReadResponseAsync<ClaimAccountResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<AccountDnsFilterResponse>> GetDnsFiltersAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "account/dns-filters");
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<AccountDnsFilterResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<UpdateAccountDnsFiltersResponse>> UpdateDnsFiltersAsync(DnsFilterPatch patch, CancellationToken cancellationToken = default)
    {
        // Pro-gated: requires Authorization: Bearer <DeviceToken> in addition to Deviceid. Until Phase 4
        // (auth) lands, SyncVpnDeviceToken is null and this will 401 - callers should surface that as
        // "not logged in" rather than a generic failure.
        UpdateAccountDnsFiltersRequest requestBody = new() { Filters = patch };
        using HttpRequestMessage request = CreateRequest(HttpMethod.Patch, "account/dns-filters", requireDeviceToken: true);
        request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<UpdateAccountDnsFiltersResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<PlanListResponse>> GetPlansAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "plans");
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<PlanListResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<PurchaseResponse>> SubmitPurchaseAsync(PurchaseRequest request, CancellationToken cancellationToken = default)
    {
        // Guest purchases don't require a device token, but a logged-in device should still send its
        // bearer if it has one so the backend can attach the purchase to that user - attach
        // opportunistically rather than requiring it.
        using HttpRequestMessage httpRequest = CreateRequest(HttpMethod.Post, "purchases", attachDeviceTokenIfAvailable: true);
        httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        return await ReadResponseAsync<PurchaseResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<TransactionListResponse>> GetTransactionsAsync(int page = 1, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, $"transactions?page={page}", requireDeviceToken: true);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
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

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);

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

    public async Task<ApiResponseResult<LoginResponse>> VerifyTwoFactorAsync(TwoFactorVerifyRequest request, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage httpRequest = CreateRequest(HttpMethod.Post, "auth/2fa/verify");
        httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        return await ReadResponseAsync<LoginResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<AuthenticatedDeviceResponse>> GetAuthenticatedDeviceAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "auth/me", requireDeviceToken: true);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<AuthenticatedDeviceResponse>(response, cancellationToken);
    }

    public async Task<ApiResponseResult<LogoutResponse>> LogoutAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, "auth/logout", requireDeviceToken: true);
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<LogoutResponse>(response, cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, bool requireDeviceToken = false, bool attachDeviceTokenIfAvailable = false)
    {
        HttpRequestMessage request = new(method, path);

        if (_settings.SyncVpnDeviceId is { Length: > 0 } deviceId)
        {
            request.Headers.Add(DeviceIdHeaderName, deviceId);
        }

        if ((requireDeviceToken || attachDeviceTokenIfAvailable) && _settings.SyncVpnDeviceToken is { Length: > 0 } deviceToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", deviceToken);
        }

        return request;
    }

    private static async Task<ApiResponseResult<T>> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponseResult<T>.Fail(response, body);
        }

        T? value = JsonConvert.DeserializeObject<T>(body);
        return ApiResponseResult<T>.Ok(response, value);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
