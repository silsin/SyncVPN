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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SyncVPN.Logging.Contracts;
using SyncVPN.ProcessCommunication.Common.Channels;
using SyncVPN.ProcessCommunication.Common.Registration;
using SyncVPN.ProcessCommunication.Common.Tests;
using SyncVPN.ProcessCommunication.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Controllers;
using SyncVPN.ProcessCommunication.Contracts.Registration;

namespace SyncVPN.ProcessCommunication.Service.Tests
{
    [TestClass]
    public class ServiceGrpcClientTest
    {
        private const int APP_SERVER_PORT = 3;

        private ILogger _logger;
        private IGrpcChannelWrapper _grpcChannelWrapper;
        private IGrpcChannelWrapperFactory _grpcChannelWrapperFactory;
        private IAppServerPortRegister _appServerPortRegister;
        private IServiceGrpcClient _serviceGrpcClient;

        [TestInitialize]
        public void Initialize()
        {
            _logger = Substitute.For<ILogger>();
            _grpcChannelWrapper = Substitute.For<IGrpcChannelWrapper>();
            _grpcChannelWrapper.CreateService<IVpnController>().Returns(c => Substitute.For<IVpnController>());
            _grpcChannelWrapperFactory = Substitute.For<IGrpcChannelWrapperFactory>();
            _grpcChannelWrapperFactory.Create(Arg.Any<int>()).Returns(_grpcChannelWrapper);
            _appServerPortRegister = Substitute.For<IAppServerPortRegister>();
            _serviceGrpcClient = new ServiceGrpcClient(_logger, _grpcChannelWrapperFactory, _appServerPortRegister);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _logger = null;
            _grpcChannelWrapper = null;
            _grpcChannelWrapperFactory = null;
            _appServerPortRegister = null;
            _serviceGrpcClient = null;
        }

        [TestMethod]
        public async Task TestCreateAsync()
        {
            await _serviceGrpcClient.CreateAsync(APP_SERVER_PORT);

            _appServerPortRegister.Received(1).Write(Arg.Any<int>());
            _appServerPortRegister.Received(1).Write(APP_SERVER_PORT);
            Assert.IsNotNull(_serviceGrpcClient.AppController);
        }

        [TestMethod]
        public void TestCreateAsync_NoRaceConditionOccurs()
        {
            int numOfCalls = 64;

            ParallelTestRunner.ExecuteInParallel(numOfCalls,
                (int i) => _serviceGrpcClient.CreateAsync(APP_SERVER_PORT + i));

            _appServerPortRegister.Received(numOfCalls).Write(Arg.Any<int>());
            Assert.IsNotNull(_serviceGrpcClient.AppController);
            _grpcChannelWrapperFactory.Received(numOfCalls).Create(Arg.Any<int>());
        }

        [TestMethod]
        public async Task TestRecreateAsync()
        {
            await _serviceGrpcClient.CreateAsync(APP_SERVER_PORT);

            _appServerPortRegister.Received(1).Write(Arg.Any<int>());
            _appServerPortRegister.Received(1).Write(APP_SERVER_PORT);

            await _serviceGrpcClient.RecreateAsync();

            _appServerPortRegister.Received(2).Write(Arg.Any<int>());
            _appServerPortRegister.Received(2).Write(APP_SERVER_PORT);
            Assert.IsNotNull(_serviceGrpcClient.AppController);
        }

        [TestMethod]
        public async Task TestRecreateAsync_WhenPortIsNull()
        {
            await _serviceGrpcClient.RecreateAsync();

            _appServerPortRegister.Received(0).Write(Arg.Any<int>());
            Assert.IsNull(_serviceGrpcClient.AppController);
        }

        [TestMethod]
        public async Task TestRecreateAsync_NoRaceConditionOccurs()
        {
            await _serviceGrpcClient.CreateAsync(APP_SERVER_PORT);
            int numOfCalls = 64;

            ParallelTestRunner.ExecuteInParallel(numOfCalls,
                (int _) => _serviceGrpcClient.RecreateAsync());

            _appServerPortRegister.Received(numOfCalls + 1).Write(Arg.Any<int>());
            _appServerPortRegister.Received(numOfCalls + 1).Write(APP_SERVER_PORT);
            Assert.IsNotNull(_serviceGrpcClient.AppController);
            _grpcChannelWrapperFactory.Received(numOfCalls + 1).Create(Arg.Any<int>());
            await _grpcChannelWrapper.Received(numOfCalls).ShutdownAsync();
        }

        [TestMethod]
        public void TestIsRecreatable_WhenServerPortIsNull()
        {
            bool result = _serviceGrpcClient.IsRecreatable();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TestIsRecreatable_WhenServerPortHasValue()
        {
            await _serviceGrpcClient.CreateAsync(APP_SERVER_PORT);

            bool result = _serviceGrpcClient.IsRecreatable();

            Assert.IsTrue(result);
        }
    }
}