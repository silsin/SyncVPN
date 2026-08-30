/*
 * Copyright (c) 2025 Proton AG
 *
 * This file is part of SyncVPN.
 *
 * SyncVPN is free software:
 you can redistribute it and/or modify
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
using NSubstitute;
using SyncVPN.Api;
using SyncVPN.Api.Contracts;
using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Installers;
using SyncVPN.Client.Localization.Building;
using SyncVPN.Client.Logic.Auth.Srp.Contracts;
using SyncVPN.Client.Services.Bootstrapping.Activators;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Bases.Contracts;
using SyncVPN.Client.UI.Main.Sidebar.Search.Contracts;
using SyncVPN.Common.Legacy.OS.Net.Http;
using RichardSzalay.MockHttp;
using File = System.IO.File;

namespace SyncVPN.Integration.Tests;

public class TestBase
{
    private IContainer? _container;

    protected MockHttpMessageHandler? MessageHandler = new();
    protected TestSrpProofGenerator SrpProofGenerator { get; } = new();

    public T Resolve<T>() => _container.Resolve<T>();

    [TestCleanup]
    public virtual void Cleanup()
    {
        _container?.Dispose();
    }

    protected void InitializeContainer()
    {
        ContainerBuilder builder = new();

        builder.RegisterModule<AppModule>();

        builder.Register(_ => SrpProofGenerator).As<ISrpProofGenerator>().SingleInstance();
        builder.Register(_ => Substitute.For<IUIThreadDispatcher>()).As<IUIThreadDispatcher>().SingleInstance();
        builder.Register(_ => Substitute.For<IDetailsViewNavigator>()).As<IDetailsViewNavigator>().SingleInstance();
        builder.Register(_ => Substitute.For<IMainViewNavigator>()).As<IMainViewNavigator>().SingleInstance();
        builder.Register(_ => Substitute.For<IReportIssueViewNavigator>()).As<IReportIssueViewNavigator>().SingleInstance();
        builder.Register(_ => Substitute.For<ISettingsViewNavigator>()).As<ISettingsViewNavigator>().SingleInstance();
        builder.Register(_ => Substitute.For<IUpsellCarouselViewNavigator>()).As<IUpsellCarouselViewNavigator>().SingleInstance();
        builder.Register(_ => Substitute.For<IConnectionsViewNavigator>()).As<IConnectionsViewNavigator>().SingleInstance();
        builder.Register(_ => Substitute.For<IEventMessageSender>()).As<IEventMessageSender>().SingleInstance();
        builder.Register(_ => Substitute.For<ILocalizerFactory>()).As<ILocalizerFactory>().SingleInstance();
        builder.Register(_ => Substitute.For<IAppStartupActivator>()).As<IAppStartupActivator>().SingleInstance();
        builder.Register(_ => Substitute.For<IMainWindowViewNavigator>()).As<IMainWindowViewNavigator>().SingleInstance();
        builder.Register(_ => Substitute.For<ISidebarViewNavigator>()).As<ISidebarViewNavigator>().SingleInstance();
        builder.Register(_ => Substitute.For<ISearchInputReceiver>()).As<ISearchInputReceiver>().SingleInstance();
        builder.Register(_ => Substitute.For<IEnumerable<IConnectionPage>>()).As<IEnumerable<IConnectionPage>>().SingleInstance();

        HttpClient httpClient = MessageHandler!.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost");

        builder.Register(_ =>
        {
            IFileDownloadHttpClientFactory httpClients = Substitute.For<IFileDownloadHttpClientFactory>();
            httpClients.GetHttpClientWithTlsPinning().Returns(new WrappedHttpClient(httpClient));
            return httpClients;
        }).As<IFileDownloadHttpClientFactory>();

        builder.Register(_ =>
        {
            IApiHttpClientFactory httpClientFactory = Substitute.For<IApiHttpClientFactory>();
            httpClientFactory.GetApiHttpClientWithCache().Returns(httpClient);
            httpClientFactory.GetApiHttpClientWithoutCache().Returns(httpClient);
            return httpClientFactory;
        }).As<IApiHttpClientFactory>();

        _container = builder.Build();
    }

    protected string GetJsonMock(string name)
    {
        return File.ReadAllText($"TestData\\{name}.json");
    }
}