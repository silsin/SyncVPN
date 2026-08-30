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

using System.Net.Http;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.Handlers.Retries;
using SyncVPN.Api.Handlers.StackBuilders;
using SyncVPN.Api.Handlers.TlsPinning;
using SyncVPN.Api.Handlers;
using SyncVPN.Common.Legacy.OS.Net.Http;

namespace SyncVPN.Api
{
    public class FileDownloadHttpClientFactory : IFileDownloadHttpClientFactory
    {
        private readonly RetryingHandler _retryingHandler;
        private readonly DnsHandler _dnsHandler;
        private readonly LoggingHandlerBase _loggingHandlerBase;
        private readonly TlsPinnedCertificateHandler _tlsPinnedCertificateHandler;
        private readonly IHttpClients _httpClients;
        private readonly IApiAppVersion _apiAppVersion;

        public FileDownloadHttpClientFactory(
            RetryingHandler retryingHandler,
            DnsHandler dnsHandler,
            LoggingHandlerBase loggingHandlerBase,
            TlsPinnedCertificateHandler tlsPinnedCertificateHandler,
            IHttpClients httpClients,
            IApiAppVersion apiAppVersion)
        {
            _retryingHandler = retryingHandler;
            _dnsHandler = dnsHandler;
            _loggingHandlerBase = loggingHandlerBase;
            _tlsPinnedCertificateHandler = tlsPinnedCertificateHandler;
            _httpClients = httpClients;
            _apiAppVersion = apiAppVersion;
        }

        public IHttpClient GetHttpClientWithTlsPinning()
        {
            HttpMessageHandler innerHandler = new HttpMessageHandlerStackBuilder()
                .AddDelegatingHandler(_retryingHandler)
                .AddDelegatingHandler(_dnsHandler)
                .AddDelegatingHandler(_loggingHandlerBase)
                .AddLastHandler(_tlsPinnedCertificateHandler)
                .Build();

            return _httpClients.Client(innerHandler, _apiAppVersion.UserAgent);
        }
    }
}