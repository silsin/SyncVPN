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

using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using Autofac;
using SyncVPN.Api.Installers;
using SyncVPN.Api.V2.Installers;
using SyncVPN.Common.Installers.Extensions;
using SyncVPN.Common.Legacy.OS.Processes;
using SyncVPN.Common.Legacy.Vpn;
using SyncVPN.Configurations.Contracts;
using SyncVPN.Configurations.Installers;
using SyncVPN.Crypto.Installers;
using SyncVPN.IPv6.Installers;
using SyncVPN.IssueReporting.Static;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppServiceLogs;
using SyncVPN.Logging.Events;
using SyncVPN.Logging.Installers;
using SyncVPN.Native.PInvoke;
using SyncVPN.OperatingSystems.Network.Installers;
using SyncVPN.Service.Settings;
using SyncVPN.Service.Vpn;
using SyncVPN.Update.Installers;
using SyncVPN.Vpn.Common;
using SyncVPN.Vpn.OpenVpn;

namespace SyncVPN.Service.Start;

internal class Bootstrapper
{
    private IContainer _container;
    private T Resolve<T>() => _container.Resolve<T>();

    public Bootstrapper()
    {
        GlobalExceptionHandler.Initialize();
        IssueReportingInitializer.Run();
    }

    public void Initialize()
    {
        SetDllDirectories();
        Configure();
        PrepareDirectories();
        Start();
    }

    private void Configure()
    {
        ContainerBuilder builder = new();
        builder.RegisterLoggerConfiguration(c => c.ServiceLogsFilePath)
               .RegisterModule<CryptoModule>()
               .RegisterModule<ServiceModule>()
               .RegisterModule<ApiModule>()
               .RegisterModule<ApiV2Module>()
               .RegisterModule<NetworkModule>()
               .RegisterModule<ConfigurationsModule>()
               .RegisterAssemblyModule<LoggingModule>()
               .RegisterAssemblyModule<IPv6Module>()
               .RegisterAssemblyModule<UpdateModule>();
        _container = builder.Build();
    } 

    private void PrepareDirectories()
    {
        IStaticConfiguration staticConfig = Resolve<IStaticConfiguration>();

        Directory.CreateDirectory(staticConfig.ServiceLogsFolder);
        Directory.CreateDirectory(staticConfig.OpenVpn.TlsExportCertFolder);
    }

    private void Start()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        RegisterEvents();

        Resolve<ILogCleaner>().Clean(Resolve<IStaticConfiguration>().ServiceLogsFolder, 10);

        VpnService vpnService = Resolve<VpnService>();
        ServiceBase.Run(vpnService);
        vpnService.CancellationToken.WaitHandle.WaitOne();

        Resolve<ILogger>().Info<AppServiceStopLog>("=== SyncVPN Service has exited ===");
    }

    private void RegisterEvents()
    {
        Resolve<IVpnConnection>().StateChanged += (_, e) =>
        {
            VpnState state = e.Data;
            IEnumerable<IVpnStateAware> instances = Resolve<IEnumerable<IVpnStateAware>>();
            foreach (IVpnStateAware instance in instances)
            {
                switch (state.Status)
                {
                    case VpnStatus.Connecting:
                    case VpnStatus.Reconnecting:
                        instance.OnVpnConnecting(state);
                        break;
                    case VpnStatus.Connected:
                        instance.OnVpnConnected(state);
                        break;
                    case VpnStatus.Disconnecting:
                    case VpnStatus.Disconnected:
                        instance.OnVpnDisconnected(state);
                        break;
                    case VpnStatus.AssigningIp:
                        instance.AssigningIp(state);
                        break;
                }
            }
        };

        Resolve<IServiceSettings>().SettingsChanged += (_, e) =>
        {
            IEnumerable<IServiceSettingsAware> instances = Resolve<IEnumerable<IServiceSettingsAware>>();
            foreach (IServiceSettingsAware instance in instances)
            {
                instance.OnServiceSettingsChanged(e);
            }

            IssueReportingInitializer.SetEnabled(e.IsShareCrashReportsEnabled);
        };
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        IStaticConfiguration config = Resolve<IStaticConfiguration>();
        IOsProcesses processes = Resolve<IOsProcesses>();
        Resolve<IVpnConnection>().Disconnect();
        Resolve<OpenVpnProcess>().Stop();
        processes.KillProcesses(config.ClientName);
    }

    private static void SetDllDirectories()
    {
        Kernel32.SetDefaultDllDirectories(Kernel32.SetDefaultDllDirectoriesFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
    }
}