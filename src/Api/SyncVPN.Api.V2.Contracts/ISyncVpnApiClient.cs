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

using System.Threading;
using System.Threading.Tasks;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts.Account;
using SyncVPN.Api.V2.Contracts.Auth;
using SyncVPN.Api.V2.Contracts.Billing;
using SyncVPN.Api.V2.Contracts.Devices;
using SyncVPN.Api.V2.Contracts.Dns;
using SyncVPN.Api.V2.Contracts.Plans;
using SyncVPN.Api.V2.Contracts.Purchases;
using SyncVPN.Api.V2.Contracts.Servers;
using SyncVPN.Api.V2.Contracts.Transactions;

namespace SyncVPN.Api.V2.Contracts;

// Client for the new SyncVPN-owned backend (https://syncvpn.com/api), as opposed to
// IApiClient/ApiClient which talk to Proton's legacy vpn-api.proton.me.
// Grown one capability at a time as each migration phase lands - see the migration plan.
public interface ISyncVpnApiClient
{
    Task<ApiResponseResult<RegisterDeviceResponse>> RegisterDeviceAsync(RegisterDeviceRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<CountryListResponse>> GetCountriesAsync(CancellationToken cancellationToken = default);

    Task<ApiResponseResult<BillingCountryListResponse>> GetBillingCountriesAsync(CancellationToken cancellationToken = default);

    Task<ApiResponseResult<ServerListResponse>> GetServersAsync(CancellationToken cancellationToken = default);

    Task<ApiResponseResult<ServerListResponse>> GetProServersAsync(CancellationToken cancellationToken = default);

    Task<ApiResponseResult<FavoriteServersResponse>> GetFavoriteServersAsync(CancellationToken cancellationToken = default);

    Task<ApiResponseResult<FavoriteServerActionResponse>> AddFavoriteServerAsync(long serverId, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<RemoveFavoriteServerResponse>> RemoveFavoriteServerAsync(long serverId, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<RateServerResponse>> RateServerAsync(long serverId, int rate, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<ClaimAccountResponse>> ClaimAccountAsync(ClaimAccountRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<ClaimAccountResponse>> ClaimAccountByCityAsync(ClaimAccountByCityRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<AccountDnsFilterResponse>> GetDnsFiltersAsync(CancellationToken cancellationToken = default);

    Task<ApiResponseResult<UpdateAccountDnsFiltersResponse>> UpdateDnsFiltersAsync(DnsFilterPatch patch, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<PlanListResponse>> GetPlansAsync(CancellationToken cancellationToken = default);

    Task<ApiResponseResult<PurchaseResponse>> SubmitPurchaseAsync(PurchaseRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<TransactionListResponse>> GetTransactionsAsync(int page = 1, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<LoginAttemptResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<LoginAttemptResponse>> CodeLoginAsync(CodeLoginRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<LoginResponse>> VerifyTwoFactorAsync(TwoFactorVerifyRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponseResult<AuthenticatedDeviceResponse>> GetAuthenticatedDeviceAsync(CancellationToken cancellationToken = default);

    Task<ApiResponseResult<LogoutResponse>> LogoutAsync(CancellationToken cancellationToken = default);
}
