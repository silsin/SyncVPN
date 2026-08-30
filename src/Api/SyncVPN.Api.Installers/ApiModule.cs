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

using Autofac;
using SyncVPN.Api.Contracts;
using SyncVPN.Api.Deserializers;
using SyncVPN.Api.Handlers;
using SyncVPN.Api.Handlers.Retries;
using SyncVPN.Api.Handlers.TlsPinning;
using SyncVPN.Common.Legacy.OS.Net.Http;

namespace SyncVPN.Api.Installers
{
    public class ApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TokenHttpClientFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ApiClient>().As<IApiClient>().SingleInstance();
            builder.RegisterType<ApiHttpClientFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<SleepDurationProvider>().SingleInstance();
            builder.RegisterType<RetryPolicyProvider>().As<IRetryPolicyProvider>().SingleInstance();
            builder.RegisterType<RetryCountProvider>().As<IRetryCountProvider>().SingleInstance();
            builder.RegisterType<RequestTimeoutProvider>().As<IRequestTimeoutProvider>().SingleInstance();
            builder.RegisterType<BaseResponseMessageDeserializer>().As<IBaseResponseMessageDeserializer>().SingleInstance();
            builder.RegisterType<ApiHostProvider>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<CertificateValidator>().As<ICertificateValidator>().SingleInstance();
            builder.RegisterType<FileDownloadHttpClientFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UpdateHttpClientFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ApiAppVersion>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<TokenClient>().As<ITokenClient>().SingleInstance();
            builder.RegisterType<ReportClientUriProvider>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ApiAvailabilityVerifier>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<HttpClients>().As<IHttpClients>().SingleInstance();
            builder.RegisterType<HumanVerificationHttpClientFactory>().AsImplementedInterfaces().SingleInstance();
            builder.Register(c =>
                    new CachingReportClient(
                        new ReportClient(c.Resolve<IReportClientUriProvider>())))
                .As<IReportClient>()
                .SingleInstance();
            RegisterHandlers(builder);
        }

        // Handlers are currently being used by both ApiClient and TokenClient and we need different instances
        // of the handlers for them since the stacks are different, hence the InstancePerDependency() usage
        private void RegisterHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<RetryingHandler>().As<RetryingHandlerBase>().AsSelf().InstancePerDependency();
            builder.RegisterType<LoggingHandler>().As<LoggingHandlerBase>().AsSelf().InstancePerDependency();
            builder.RegisterType<HumanVerificationHandler>().As<HumanVerificationHandlerBase>().AsSelf().InstancePerDependency();
            builder.RegisterType<CancellingHandler>().As<CancellingHandlerBase>().AsSelf().InstancePerDependency();

            builder.RegisterType<AlternativeHostHandler>().AsImplementedInterfaces().AsSelf().InstancePerDependency();
            
            builder.RegisterType<DnsHandler>().InstancePerDependency();
            builder.RegisterType<OutdatedAppHandler>().InstancePerDependency();
            builder.RegisterType<UnauthorizedResponseHandler>().InstancePerDependency();
            builder.RegisterType<TlsPinnedCertificateHandler>().InstancePerDependency();
            builder.RegisterType<SslCertificateHandler>().InstancePerDependency();
        }
    }
}