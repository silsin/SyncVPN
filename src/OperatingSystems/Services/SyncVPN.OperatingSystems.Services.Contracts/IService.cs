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

namespace SyncVPN.OperatingSystems.Services.Contracts;

public interface IService
{
    string Name { get; }

    bool IsCreated();
    void Create(ServiceCreationOptions options);

    bool IsEnabled();
    void Enable();
    void UpdatePathAndArgs(string pathAndArgs);
    string? GetBinaryPath();

    bool IsRunning();
    bool IsStopped();

    bool Start();
    bool StartWithRetry();
    Task<bool> StartAsync(CancellationToken cancellationToken);

    bool Stop();
    bool StopWithRetry();
    Task<bool> StopAsync(CancellationToken cancellationToken);

    ServiceStatus? GetStatus();
}