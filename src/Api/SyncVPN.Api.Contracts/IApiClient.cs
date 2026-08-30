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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SyncVPN.Api.Contracts.Announcements;
using SyncVPN.Api.Contracts.Auth;
using SyncVPN.Api.Contracts.Certificates;
using SyncVPN.Api.Contracts.Common;
using SyncVPN.Api.Contracts.Events;
using SyncVPN.Api.Contracts.Features;
using SyncVPN.Api.Contracts.Geographical;
using SyncVPN.Api.Contracts.NpsSurvey;
using SyncVPN.Api.Contracts.Partners;
using SyncVPN.Api.Contracts.ReportAnIssue;
using SyncVPN.Api.Contracts.Servers;
using SyncVPN.Api.Contracts.Streaming;
using SyncVPN.Api.Contracts.Users;
using SyncVPN.Api.Contracts.VpnConfig;
using SyncVPN.Common.Core.Geographical;
using SyncVPN.Common.Core.StatisticalEvents;

namespace SyncVPN.Api.Contracts;

public interface IApiClient : IClientBase
{
    Task<ApiResponseResult<LocalizedLocationsResponse>> GetCityNamesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponseResult<UnauthSessionResponse>> PostUnauthSessionAsync(CancellationToken cancellationToken = default);
    Task<ApiResponseResult<AuthResponse>> GetAuthResponse(AuthRequest authRequest, CancellationToken cancellationToken = default);
    Task<ApiResponseResult<AuthInfoResponse>> GetAuthInfoResponse(AuthInfoRequest authInfoRequest, CancellationToken cancellationToken = default);
    Task<ApiResponseResult<BaseResponse>> GetTwoFactorAuthResponse(TwoFactorRequest twoFactorRequest, string accessToken, string uid, CancellationToken cancellationToken = default);
    Task<ApiResponseResult<PhysicalServerWrapperResponse>> GetServerAsync(string serverId);
    Task<ApiResponseResult<VpnInfoWrapperResponse>> GetVpnInfoResponse(CancellationToken cancellationToken = default);
    Task<ApiResponseResult<BaseResponse>> GetLogoutResponse();
    Task<ApiResponseResult<EventResponse>> GetEventResponse(string lastId = default);
    Task<ApiResponseResult<ServersResponse>> GetServersAsync(DeviceLocation? deviceLocation, IEnumerable<string> favoriteServerIds = null, CancellationToken cancellationToken = default);
    Task<ApiResponseResult<byte[]>> GetServerLoadsAndStatusBinaryStringAsync(string statusId, CancellationToken cancellationToken = default);
    Task<ApiResponseResult<ServerCountResponse>> GetServersCountAsync();
    Task<ApiResponseResult<ReportAnIssueFormResponse>> GetReportAnIssueFormData();
    Task<ApiResponseResult<ServersResponse>> GetServerLoadsAsync(DeviceLocation? deviceLocation, CancellationToken cancellationToken);
    Task<ApiResponseResult<DeviceLocationResponse>> GetLocationDataAsync();
    Task<ApiResponseResult<BaseResponse>> ReportBugAsync(IEnumerable<KeyValuePair<string, string>> fields, IEnumerable<File> files);
    Task<ApiResponseResult<VpnConfigResponse>> GetVpnConfigAsync(DeviceLocation? deviceLocation, CancellationToken cancellationToken = default);
    Task<ApiResponseResult<AnnouncementsResponse>> GetAnnouncementsAsync(AnnouncementsRequest request);
    Task<ApiResponseResult<StreamingServicesResponse>> GetStreamingServicesAsync();
    Task<ApiResponseResult<PartnersResponse>> GetPartnersAsync();
    Task<ApiResponseResult<CertificateResponse>> RequestConnectionCertificateAsync(CertificateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponseResult<BaseResponse>> ApplyPromoCodeAsync(PromoCodeRequest promoCodeRequest);
    Task<ApiResponseResult<ForkedAuthSessionResponse>> ForkAuthSessionAsync(AuthForkSessionRequest request);
    Task<ApiResponseResult<BaseResponse>> PostUnauthenticatedStatisticalEventsAsync(StatisticalEventsBatch statisticalEvents);
    Task<ApiResponseResult<BaseResponse>> PostAuthenticatedStatisticalEventsAsync(StatisticalEventsBatch statisticalEvents);
    Task<ApiResponseResult<UsersResponse>> GetUserAsync(CancellationToken cancellationToken = default);
    Task<ApiResponseResult<Ipv6FragmentsResponse>> GetIpv6FragmentsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponseResult<FeatureFlagsResponse>> GetFeatureFlagsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponseResult<LookupServerResponse>> GetServerByNameAsync(string serverName, DeviceLocation? deviceLocation);
    Task<ApiResponseResult<BaseResponse>> SubmitNpsSurveyAsync(NpsSurveyRequest request, DeviceLocation? deviceLocation);
    Task<ApiResponseResult<BaseResponse>> DismissNpsSurveyAsync(DeviceLocation? deviceLocation);
}