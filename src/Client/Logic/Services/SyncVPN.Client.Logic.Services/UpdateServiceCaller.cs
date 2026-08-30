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

using SyncVPN.Client.Contracts.ProcessCommunication;
using SyncVPN.Client.Logic.Services.Contracts;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Logging.Contracts;
using SyncVPN.ProcessCommunication.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Controllers;
using SyncVPN.ProcessCommunication.Contracts.Entities.Update;

namespace SyncVPN.Client.Logic.Services;

public class UpdateServiceCaller : ServiceCallerBase<IUpdateController>, IUpdateServiceCaller
{
    public UpdateServiceCaller(ILogger logger, IGrpcClient grpcClient,
        Lazy<IServiceCommunicationErrorHandler> serviceCommunicationErrorHandler)
        : base(logger, grpcClient, serviceCommunicationErrorHandler)
    {
    }

    public Task CheckForUpdateAsync(UpdateSettingsIpcEntity updateSettingsIpcEntity)
    {
        return InvokeAsync((c, ct) => c.CheckForUpdate(updateSettingsIpcEntity, ct).Wrap());
    }

    public Task StartAutoUpdateAsync()
    {
        StartAutoUpdateIpcEntity startAutoUpdateIpcEntity = new()
        {
            RetryId = Guid.NewGuid()
        };

        return InvokeAsync((c, ct) => c.StartAutoUpdate(startAutoUpdateIpcEntity, ct).Wrap());
    }
}