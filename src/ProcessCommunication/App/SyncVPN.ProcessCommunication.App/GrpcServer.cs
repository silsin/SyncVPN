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

using ProtoBuf.Grpc.Server;
using SyncVPN.Logging.Contracts;
using SyncVPN.ProcessCommunication.Common;
using SyncVPN.ProcessCommunication.Contracts.Controllers;
using static Grpc.Core.Server;

namespace SyncVPN.ProcessCommunication.App
{
    public class GrpcServer : GrpcServerBase
    {
        private readonly IAppController _appController;

        public GrpcServer(ILogger logger, IAppController appController)
            : base(logger)
        {
            _appController = appController;
        }

        protected override void RegisterServices(ServiceDefinitionCollection services)
        {
            services.AddCodeFirst<IAppController>(_appController);
        }
    }
}