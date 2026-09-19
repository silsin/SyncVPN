/*
 * Copyright (c) 2025 Proton AG
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
using Autofac.Builder;
using SyncVPN.Api.Installers;
using SyncVPN.Api.V2.Installers;
using SyncVPN.Client.Commands;
using SyncVPN.Client.Core.Bases.Helpers;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.EventMessaging.Installers;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Files.Installers;
using SyncVPN.Client.Handlers;
using SyncVPN.Client.Handlers.Bases;
using SyncVPN.Client.Localization.Installers;
using SyncVPN.Client.Logic.Announcements.Installers;
using SyncVPN.Client.Logic.Auth.Installers;
using SyncVPN.Client.Logic.Connection.Installers;
using SyncVPN.Client.Logic.Feedback.Installers;
using SyncVPN.Client.Logic.Profiles.Installers;
using SyncVPN.Client.Logic.Purchases.Installers;
using SyncVPN.Client.Logic.Recents.Installers;
using SyncVPN.Client.Logic.Searches.Installers;
using SyncVPN.Client.Logic.Servers.Installers;
using SyncVPN.Client.Logic.Services.Installers;
using SyncVPN.Client.Logic.Updates.Installers;
using SyncVPN.Client.Logic.Users.Installers;
using SyncVPN.Client.Models.Announcements;
using SyncVPN.Client.Notifications.Installers;
using SyncVPN.Client.Services;
using SyncVPN.Client.Services.Activation;
using SyncVPN.Client.Services.Bootstrapping;
using SyncVPN.Client.Services.Bootstrapping.Activators;
using SyncVPN.Client.Services.Browsing;
using SyncVPN.Client.Services.DefaultConnections;
using SyncVPN.Client.Services.Dispatching;
using SyncVPN.Client.Services.DnsFilters;
using SyncVPN.Client.Services.Edition;
using SyncVPN.Client.Services.Enabling;
using SyncVPN.Client.Services.FreeServers;
using SyncVPN.Client.Services.ServerPing;
using SyncVPN.Client.Services.Lifecycle;
using SyncVPN.Client.Services.LocationExclusion;
using SyncVPN.Client.Services.Mapping;
using SyncVPN.Client.Services.Navigation;
using SyncVPN.Client.Services.PortForwarding;
using SyncVPN.Client.Services.ProcessCommunication;
using SyncVPN.Client.Services.Selection;
using SyncVPN.Client.Services.SignoutHandling;
using SyncVPN.Client.Services.TeachingTips;
using SyncVPN.Client.Services.Upselling;
using SyncVPN.Client.Services.Validation;
using SyncVPN.Client.Settings.Installers;
using SyncVPN.Client.UI;
using SyncVPN.Client.UI.Dialogs.DebugTools;
using SyncVPN.Client.UI.Dialogs.NpsSurvey;
using SyncVPN.Client.UI.Dialogs.OneTimeAnnouncement;
using SyncVPN.Client.UI.Dialogs.ReportIssue;
using SyncVPN.Client.UI.Dialogs.ReportIssue.Pages;
using SyncVPN.Client.UI.Dialogs.Tray;
using SyncVPN.Client.UI.Dialogs.Tray.Pages;
using SyncVPN.Client.UI.Dialogs.Troubleshooting;
using SyncVPN.Client.UI.Dialogs.Upsell;
using SyncVPN.Client.UI.Dialogs.Upsell.Features;
using SyncVPN.Client.UI.Login;
using SyncVPN.Client.UI.Login.Components;
using SyncVPN.Client.UI.Login.Overlays;
using SyncVPN.Client.UI.Login.Pages;
using SyncVPN.Client.UI.Main;
using SyncVPN.Client.UI.Main.Components;
using SyncVPN.Client.UI.Main.Components.Banners;
using SyncVPN.Client.UI.Main.FeatureIcons;
using SyncVPN.Client.UI.Main.Features.KillSwitch;
using SyncVPN.Client.UI.Main.Features.NetShield;
using SyncVPN.Client.UI.Main.Features.PortForwarding;
using SyncVPN.Client.UI.Main.Features.SplitTunneling;
using SyncVPN.Client.UI.Main.Home;
using SyncVPN.Client.UI.Main.Home.FreeServers;
using SyncVPN.Client.UI.Main.Home.Card;
using SyncVPN.Client.UI.Main.Home.Card.DefaultConnections;
using SyncVPN.Client.UI.Main.Home.Details;
using SyncVPN.Client.UI.Main.Home.Details.Connection;
using SyncVPN.Client.UI.Main.Home.Details.Flyouts;
using SyncVPN.Client.UI.Main.Home.Details.Location;
using SyncVPN.Client.UI.Main.Home.Status;
using SyncVPN.Client.UI.Main.Home.Upsell;
using SyncVPN.Client.UI.Main.Map;
using SyncVPN.Client.UI.Main.Profiles;
using SyncVPN.Client.UI.Main.Profiles.Components;
using SyncVPN.Client.UI.Main.Settings;
using SyncVPN.Client.UI.Main.Settings.Connection;
using SyncVPN.Client.UI.Main.Settings.Pages;
using SyncVPN.Client.UI.Main.Settings.Pages.About;
using SyncVPN.Client.UI.Main.Settings.Pages.Advanced;
using SyncVPN.Client.UI.Main.Settings.Pages.Connection;
using SyncVPN.Client.UI.Main.Settings.Pages.ConnectionPreferences;
using SyncVPN.Client.UI.Main.Sidebar;
using SyncVPN.Client.UI.Main.Sidebar.Connections;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Countries;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Countries.All;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Countries.P2P;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Countries.SecureCore;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Countries.Tor;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Gateways;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Profiles;
using SyncVPN.Client.UI.Main.Sidebar.Connections.Recents;
using SyncVPN.Client.UI.Main.Sidebar.Search;
using SyncVPN.Client.UI.Main.Store;
using SyncVPN.Client.UI.Main.Widgets;
using SyncVPN.Client.UI.Overlays.Information;
using SyncVPN.Client.UI.Overlays.Information.Notification;
using SyncVPN.Client.UI.Overlays.Selection;
using SyncVPN.Client.UI.Overlays.Store;
using SyncVPN.Client.UI.Overlays.Upsell;
using SyncVPN.Client.UI.Overlays.Welcome;
using SyncVPN.Client.UI.Overlays.WhatsNew;
using SyncVPN.Client.UI.Tray;
using SyncVPN.Client.UI.Update;
using SyncVPN.Client.UnsecureWifiDetection.Installers;
using SyncVPN.Common.Legacy.OS.DeviceIds;
using SyncVPN.Common.Legacy.OS.Processes;
using SyncVPN.Common.Legacy.OS.Systems;
using SyncVPN.Configurations.Installers;
using SyncVPN.Crypto.Installers;
using SyncVPN.Dns.Installers;
using SyncVPN.EntityMapping.Installers;
using SyncVPN.Files.Installers;
using SyncVPN.IPv6.Installers;
using SyncVPN.IssueReporting.Installers;
using SyncVPN.Logging.Installers;
using SyncVPN.NetworkTimeProtocols.Installers;
using SyncVPN.OperatingSystems.Antiviruses.Installers;
using SyncVPN.OperatingSystems.Network.Installers;
using SyncVPN.OperatingSystems.Processes.Installers;
using SyncVPN.OperatingSystems.Registries.Installers;
using SyncVPN.OperatingSystems.Services.Installers;
using SyncVPN.OperatingSystems.WebAuthn.Installers;
using SyncVPN.ProcessCommunication.Client.Installers;
using SyncVPN.ProcessCommunication.Installers;
using SyncVPN.Serialization.Installers;
using SyncVPN.StatisticalEvents.Installers;

namespace SyncVPN.Client.Installers;

public class AppModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterLoggerConfiguration(c => c.ClientLogsFilePath);

        RegisterExternalModules(builder);
        RegisterLocalServices(builder);
        RegisterLocalHandlers(builder);
        RegisterViewModels(builder);
        RegisterExternalServices(builder);
        RegisterCommands(builder);
    }

    private static IRegistrationBuilder<TViewModel, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterViewModel<TViewModel>(ContainerBuilder builder)
        where TViewModel : ViewModelBase
    {
        return builder.RegisterType<TViewModel>().AsSelf().AsImplementedInterfaces().SingleInstance();
    }

    private void RegisterExternalServices(ContainerBuilder builder)
    {
        builder.RegisterType<SystemProcesses>().As<IOsProcesses>().SingleInstance();
        builder.RegisterType<SystemState>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<DeviceIdCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ServiceEnabler>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<BaseFilteringEngineDialogHandler>().AsImplementedInterfaces().SingleInstance();
    }

    private void RegisterExternalModules(ContainerBuilder builder)
    {
        builder.RegisterModule<EventMessageReceiverActivationModule>()
               .RegisterModule<LoggingModule>()
               .RegisterModule<RegistriesModule>()
               .RegisterModule<ProcessesModule>()
               .RegisterModule<ServicesModule>()
               .RegisterModule<WebAuthnModule>()
               .RegisterModule<ServicesLogicModule>()
               .RegisterModule<ConnectionLogicModule>()
               .RegisterModule<ClientProcessCommunicationModule>()
               .RegisterModule<EntityMappingModule>()
               .RegisterModule<ProcessCommunicationModule>()
               .RegisterModule<LocalizationModule>()
               .RegisterModule<EventMessagingModule>()
               .RegisterModule<RecentsLogicModule>()
               .RegisterModule<ProfilesLogicModule>()
               .RegisterModule<ServersLogicModule>()
               .RegisterModule<ConfigurationsModule>()
               .RegisterModule<ApiModule>()
               .RegisterModule<ApiV2Module>()
               .RegisterModule<SettingsModule>()
               .RegisterModule<AuthLogicModule>()
               .RegisterModule<PurchasesLogicModule>()
               .RegisterModule<CryptoModule>()
               .RegisterModule<FeedbackLogicModule>()
               .RegisterModule<DnsModule>()
               .RegisterModule<SerializationModule>()
               .RegisterModule<UpdatesLogicModule>()
               .RegisterModule<IssueReportingModule>()
               .RegisterModule<NotificationsModule>()
               .RegisterModule<FilesModule>()
               .RegisterModule<ClientFilesModule>()
               .RegisterModule<PowerEventsModule>()
               .RegisterModule<UsersLogicModule>()
               .RegisterModule<AnnouncementsModule>()
               .RegisterModule<SearchesModule>()
               .RegisterModule<NetworkModule>()
               .RegisterModule<UnsecureWifiDetectionModule>()
               .RegisterModule<StatisticalEventsModule>()
               .RegisterModule<IPv6Module>()
               .RegisterModule<AntivirusesModule>()
               .RegisterModule<NetworkTimeProtocolsModule>();
    }

    private void RegisterLocalServices(ContainerBuilder builder)
    {
        builder.RegisterType<Bootstrapper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AppProtocolActivator>().AsImplementedInterfaces().SingleInstance().AutoActivate();
        builder.RegisterType<AppStartupActivator>().AsImplementedInterfaces().SingleInstance().AutoActivate();
        builder.RegisterType<FreeServersCache>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<FreeServersObserver>().AsImplementedInterfaces().SingleInstance().AutoActivate();
        builder.RegisterType<ServerPingService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<PageViewMapper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<OverlayViewMapper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UIThreadDispatcher>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UrlsBrowser>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<FilesBrowser>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AccountUpgradeUrlLauncher>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ClipboardEditor>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SignOutHandler>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<PortForwardingClipboardService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AnnouncementActivator>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<MainWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MainWindowOverlayActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MainWindowViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LoginViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<MainViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<DetailsViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SidebarViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionsViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SettingsViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ReportIssueWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ReportIssueWindowOverlayActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ReportIssueViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<UpsellCarouselWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UpsellCarouselViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<TrayAppWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<TrayAppViewNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<DebugToolsWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<TroubleshootingWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<OneTimeAnnouncementWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NpsSurveyWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ClientWindowsActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ClientViewsNavigator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ApplicationIconSelector>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ApplicationThemeSelector>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<WebBrowserAppSelector>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ConnectionGroupFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionItemFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<LocationItemFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<CommonItemFactory>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SplitTunnelingItemFactory>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<SystemTimeValidator>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ViewModelHelper>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AppExitInvoker>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<CoordinatesProvider>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ServiceCommunicationErrorHandler>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<EfficiencyModeEnabler>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<SettingsHeartbeatService>().AsImplementedInterfaces().SingleInstance().AutoActivate();

        builder.RegisterType<P2PDetectionWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<StreamingDetectionWindowActivator>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<DefaultConnectionSelectionManager>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<ExcludeLocationsManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<DnsFiltersManager>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<TeachingTipService>().AsImplementedInterfaces().SingleInstance();
    }

    private void RegisterLocalHandlers(ContainerBuilder builder)
    {
        Type handlerType = typeof(IHandler);

        builder.RegisterAssemblyTypes(handlerType.Assembly)
               .Where(handlerType.IsAssignableFrom)
               .AsImplementedInterfaces()
               .SingleInstance()
               .AutoActivate();
    }

    private void RegisterCommands(ContainerBuilder builder)
    {
        Type providerType = typeof(ICommandProvider);

        builder.RegisterAssemblyTypes(providerType.Assembly)
               .Where(providerType.IsAssignableFrom)
               .AsImplementedInterfaces()
               .SingleInstance();
    }

    private void RegisterViewModels(ContainerBuilder builder)
    {
        RegisterViewModel<MainWindowShellViewModel>(builder);

        RegisterViewModel<TrayIconComponentViewModel>(builder);

        RegisterViewModel<LoginPageViewModel>(builder);
        RegisterViewModel<SignInPageViewModel>(builder);
        RegisterViewModel<WebLoginPageViewModel>(builder);
        RegisterViewModel<TwoFactorPageViewModel>(builder);
        RegisterViewModel<CodeLoginPageViewModel>(builder);
        RegisterViewModel<LoadingPageViewModel>(builder);
        RegisterViewModel<DisableKillSwitchBannerViewModel>(builder);

        RegisterViewModel<BannerViewModel>(builder).AutoActivate();
        RegisterViewModel<ProminentBannerViewModel>(builder).AutoActivate();
        RegisterViewModel<TitleBarMenuViewModel>(builder);
        RegisterViewModel<NetShieldStatsViewModel>(builder);
        RegisterViewModel<ActivePortComponentViewModel>(builder);
        RegisterViewModel<CurrentProfileIconViewModel>(builder);
        RegisterViewModel<MainPageViewModel>(builder);
        RegisterViewModel<NoServersPageViewModel>(builder);
        RegisterViewModel<SidebarComponentViewModel>(builder).AutoActivate();
        RegisterViewModel<ConnectionsPageViewModel>(builder);
        RegisterViewModel<StorePageViewModel>(builder);
        RegisterViewModel<RecentsPageViewModel>(builder);
        RegisterViewModel<ProfilesPageViewModel>(builder);
        RegisterViewModel<GatewaysPageViewModel>(builder);
        RegisterViewModel<CountriesPageViewModel>(builder);
        RegisterViewModel<AllCountriesComponentViewModel>(builder);
        RegisterViewModel<SecureCoreCountriesComponentViewModel>(builder);
        RegisterViewModel<P2PCountriesComponentViewModel>(builder);
        RegisterViewModel<TorCountriesComponentViewModel>(builder);
        RegisterViewModel<SearchResultsPageViewModel>(builder);
        RegisterViewModel<HomeComponentViewModel>(builder);
        RegisterViewModel<HomeFreeServersSectionViewModel>(builder);
        RegisterViewModel<MapComponentViewModel>(builder).AutoActivate();
        RegisterViewModel<ConnectionCardComponentViewModel>(builder);
        RegisterViewModel<DefaultConnectionSelectorViewModel>(builder);
        RegisterViewModel<ChangeServerComponentViewModel>(builder);
        RegisterViewModel<ConnectionCardUpsellBannerViewModel>(builder);
        RegisterViewModel<ConnectionStatusGradientViewModel>(builder);
        RegisterViewModel<ConnectionStatusHeaderViewModel>(builder);
        RegisterViewModel<DetailsComponentViewModel>(builder);
        RegisterViewModel<ConnectionDetailsPageViewModel>(builder).AutoActivate();
        RegisterViewModel<LocationDetailsPageViewModel>(builder);
        RegisterViewModel<KillSwitchPageViewModel>(builder);
        RegisterViewModel<KillSwitchWidgetViewModel>(builder);
        RegisterViewModel<NetShieldPageViewModel>(builder);
        RegisterViewModel<NetShieldWidgetViewModel>(builder);
        RegisterViewModel<PortForwardingPageViewModel>(builder);
        RegisterViewModel<PortForwardingWidgetViewModel>(builder);
        RegisterViewModel<SplitTunnelingPageViewModel>(builder);
        RegisterViewModel<SplitTunnelingWidgetViewModel>(builder);
        RegisterViewModel<SettingsPageViewModel>(builder);
        RegisterViewModel<SettingsWidgetViewModel>(builder);
        RegisterViewModel<CommonSettingsPageViewModel>(builder);
        RegisterViewModel<AdvancedSettingsPageViewModel>(builder);
        RegisterViewModel<ProtocolSettingsPageViewModel>(builder);
        RegisterViewModel<ConnectionPreferencesSettingsPageViewModel>(builder);
        RegisterViewModel<VpnAcceleratorSettingsPageViewModel>(builder);
        RegisterViewModel<CustomDnsServersViewModel>(builder);
        RegisterViewModel<DebugLogsPageViewModel>(builder);
        RegisterViewModel<AboutPageViewModel>(builder);
        RegisterViewModel<LicensingViewModel>(builder);
        RegisterViewModel<CensorshipSettingsPageViewModel>(builder);
        RegisterViewModel<AutoStartupSettingsPageViewModel>(builder);
        RegisterViewModel<SideWidgetsHostComponentViewModel>(builder);
        RegisterViewModel<SpeedChartComponentViewModel>(builder);
        RegisterViewModel<ConnectionIntentSelectorViewModel>(builder);
        RegisterViewModel<ProfileIconSelectorViewModel>(builder);
        RegisterViewModel<ProfileSettingsSelectorViewModel>(builder);
        RegisterViewModel<ProfileOptionsSelectorViewModel>(builder);
        RegisterViewModel<ProfilePageViewModel>(builder);
        RegisterViewModel<UserDetailsComponentViewModel>(builder);
        RegisterViewModel<ConnectionSettingsViewModel>(builder);
        RegisterViewModel<ConnectionErrorViewModel>(builder);
        RegisterViewModel<ServiceDisabledBannerViewModel>(builder);
        RegisterViewModel<UpdateViewModel>(builder).AutoActivate();

        RegisterViewModel<ReportIssueShellViewModel>(builder);
        RegisterViewModel<ReportIssueCategoriesPageViewModel>(builder);
        RegisterViewModel<ReportIssueCategoryPageViewModel>(builder);
        RegisterViewModel<ReportIssueContactPageViewModel>(builder);
        RegisterViewModel<ReportIssueResultPageViewModel>(builder);

        RegisterViewModel<TrayAppShellViewModel>(builder);
        RegisterViewModel<TrayLoginPageViewModel>(builder);
        RegisterViewModel<TrayMainPageViewModel>(builder);

        RegisterViewModel<UpsellCarouselShellViewModel>(builder);
        RegisterViewModel<AdvancedSettingsUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<MultipleDevicesUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<NetShieldUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<P2PUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<SecureCoreUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<SpeedUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<SplitTunnelingUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<StreamingUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<TorUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<WorldwideCoverageUpsellFeaturePageViewModel>(builder);
        RegisterViewModel<ProfilesUpsellFeaturePageViewModel>(builder);

        RegisterViewModel<P2POverlayViewModel>(builder);
        RegisterViewModel<SecureCoreOverlayViewModel>(builder);
        RegisterViewModel<TorOverlayViewModel>(builder);
        RegisterViewModel<SmartRoutingOverlayViewModel>(builder);
        RegisterViewModel<ProfileOverlayViewModel>(builder);
        RegisterViewModel<ServerLoadOverlayViewModel>(builder);
        RegisterViewModel<SsoLoginOverlayViewModel>(builder);
        RegisterViewModel<OutdatedClientOverlayViewModel>(builder).AutoActivate();
        RegisterViewModel<WelcomeOverlayViewModel>(builder);
        RegisterViewModel<WelcomeToVpnPlusOverlayViewModel>(builder);
        RegisterViewModel<WelcomeToVpnUnlimitedOverlayViewModel>(builder);
        RegisterViewModel<WelcomeToVpnB2BOverlayViewModel>(builder);
        RegisterViewModel<WhatsNewOverlayViewModel>(builder);
        RegisterViewModel<IpSelectorOverlayViewModel>(builder);
        RegisterViewModel<AppSelectorOverlayViewModel>(builder);
        RegisterViewModel<StoreGuestEmailOverlayViewModel>(builder);

        RegisterViewModel<KillSwitchIconViewModel>(builder);
        RegisterViewModel<ProtocolIconViewModel>(builder);
        RegisterViewModel<NetShieldIconViewModel>(builder);
        RegisterViewModel<PortForwardingIconViewModel>(builder);
        RegisterViewModel<SplitTunnelingIconViewModel>(builder);
        RegisterViewModel<VpnAcceleratorIconViewModel>(builder);

        RegisterViewModel<FreeConnectionsOverlayViewModel>(builder);

        RegisterViewModel<IpAddressFlyoutViewModel>(builder);
        RegisterViewModel<CountryFlyoutViewModel>(builder);
        RegisterViewModel<IspFlyoutViewModel>(builder);
        RegisterViewModel<ServerLoadFlyoutViewModel>(builder);
        RegisterViewModel<ProtocolFlyoutViewModel>(builder);
        RegisterViewModel<VolumeFlyoutViewModel>(builder).AutoActivate();
        RegisterViewModel<SpeedFlyoutViewModel>(builder).AutoActivate();

        RegisterViewModel<DebugToolsShellViewModel>(builder);
        RegisterViewModel<OneTimeAnnouncementShellViewModel>(builder).AutoActivate();
        RegisterViewModel<NpsSurveyShellViewModel>(builder).AutoActivate();
        RegisterViewModel<TroubleshootingShellViewModel>(builder);

        RegisterViewModel<P2PDetectionShellViewModel>(builder);
        RegisterViewModel<StreamingDetectionShellViewModel>(builder);

        builder.RegisterType<ReleaseViewModelFactory>().SingleInstance();
    }
}