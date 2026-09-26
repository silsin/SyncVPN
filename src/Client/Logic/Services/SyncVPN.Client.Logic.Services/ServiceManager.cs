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

using SyncVPN.Client.Logic.Services.Contracts;
using SyncVPN.Configurations.Contracts;
using SyncVPN.OperatingSystems.Services.Contracts;

namespace SyncVPN.Client.Logic.Services;

public class ServiceManager : IServiceManager
{
    // The Setup installer always places the service exe next to the client exe (both copied into the
    // same "{app}\{#VersionFolder}" - see SetupBase.iss), and local F5/dev builds put them in the same
    // "src\bin" output folder too, so "wherever this client is running from" reliably locates it in both
    // cases without needing its own configured path.
    private const string ServiceExeFileName = "SyncVPNService.exe";

    private readonly IService _service;
    private readonly IServiceEnabler _serviceEnabler;
    private readonly string _serviceExePath;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ServiceManager(
        IServiceFactory serviceFactory,
        IStaticConfiguration configuration,
        IServiceEnabler serviceEnabler)
    {
        _service = serviceFactory.Get(configuration.ServiceName);
        _serviceEnabler = serviceEnabler;

        // Only worth offering to auto-create the service with this path if the exe is actually sitting
        // there - an unusual deployment layout should fall back to the old "tell the user to reinstall"
        // behavior rather than let "sc create" run against a binPath that doesn't exist.
        string candidatePath = Path.Combine(AppContext.BaseDirectory, ServiceExeFileName);
        _serviceExePath = File.Exists(candidatePath) ? candidatePath : string.Empty;
    }

    public bool IsServiceEnabled => _service.IsEnabled();

    public ServiceStatus? GetStatus()
    {
        return _service.GetStatus();
    }

    public async Task StartAsync()
    {
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        await _serviceEnabler.EnableAsync(_service, _serviceExePath).ConfigureAwait(false);

        await _service.StartAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
    }

    public Task<bool> EnableServiceAsync()
    {
        return _serviceEnabler.TryEnableAsync(_service, _serviceExePath);
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _service.Stop();
    }
}
