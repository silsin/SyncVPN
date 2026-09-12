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
using SyncVPN.Common.Legacy.Vpn;
using SyncVPN.Crypto.Contracts;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Crypto;
using SyncVPN.ProcessCommunication.Contracts.Entities.LocalAgent;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;
using SyncVPN.ProcessCommunication.EntityMapping.Vpn;

namespace SyncVPN.ProcessCommunication.EntityMapping.Tests.Vpn;

[TestClass]
public class VpnCredentialsMapperTest
{
    private IEntityMapper _entityMapper;
    private VpnCredentialsMapper _mapper;

    private AsymmetricKeyPairIpcEntity _expectedAsymmetricKeyPairIpcEntity;
    private AsymmetricKeyPair _expectedAsymmetricKeyPair;

    [TestInitialize]
    public void Initialize()
    {
        _entityMapper = Substitute.For<IEntityMapper>();
        _mapper = new(_entityMapper);

        _expectedAsymmetricKeyPairIpcEntity = new AsymmetricKeyPairIpcEntity();
        _entityMapper.Map<AsymmetricKeyPair, AsymmetricKeyPairIpcEntity>(Arg.Any<AsymmetricKeyPair>())
            .Returns(_expectedAsymmetricKeyPairIpcEntity);

        _expectedAsymmetricKeyPair = new AsymmetricKeyPair(
            new SecretKey("PVPN", KeyAlgorithm.Unknown), new PublicKey("PVPN", KeyAlgorithm.Unknown));
        _entityMapper.Map<AsymmetricKeyPairIpcEntity, AsymmetricKeyPair>(Arg.Any<AsymmetricKeyPairIpcEntity>())
            .Returns(_expectedAsymmetricKeyPair);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _entityMapper = null;
        _mapper = null;

        _expectedAsymmetricKeyPairIpcEntity = null;
        _expectedAsymmetricKeyPair = null;
    }

    [TestMethod]
    public void TestMapLeftToRight_WithCertificate()
    {
        VpnCredentials entityToTest = new("CERT",
            DateTime.UtcNow.AddDays(1),
            new AsymmetricKeyPair(
                new SecretKey("PVPN", KeyAlgorithm.Ed25519),
                new PublicKey("PVPN", KeyAlgorithm.Ed25519)),
            "username",
            "password");

        VpnCredentialsIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.ClientCertPem, result.Certificate.Pem);
        Assert.AreEqual(entityToTest.ClientCertificateExpirationDateUtc, result.Certificate.ExpirationDateUtc);
        Assert.AreEqual(_expectedAsymmetricKeyPairIpcEntity, result.ClientKeyPair);
        Assert.AreEqual(entityToTest.Username, result.Username);
        Assert.AreEqual(entityToTest.Password, result.Password);
    }

    [TestMethod]
    public void TestMapRightToLeft_WithCertificate()
    {
        VpnCredentialsIpcEntity entityToTest = new()
        {
            Certificate = CreateCertificate(),
            ClientKeyPair = new AsymmetricKeyPairIpcEntity(),
            Username = "username",
            Password = "password",
        };

        VpnCredentials result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Certificate.Pem, result.ClientCertPem);
        Assert.AreEqual(entityToTest.Certificate.ExpirationDateUtc, result.ClientCertificateExpirationDateUtc);
        Assert.AreEqual(_expectedAsymmetricKeyPair, result.ClientKeyPair);
        Assert.AreEqual(entityToTest.Username, result.Username);
        Assert.AreEqual(entityToTest.Password, result.Password);
    }

    private ConnectionCertificateIpcEntity CreateCertificate()
    {
        return new()
        {
            Pem = DateTime.UtcNow.Ticks.ToString(),
            ExpirationDateUtc = DateTime.UtcNow.AddDays(1),
        };
    }

    [TestMethod]
    public void TestMapLeftToRight_WithProvisionedConfig_NoKeyPairRequired()
    {
        VpnCredentials entityToTest = VpnCredentials.FromProvisionedConfig("[Interface]\nPrivateKey = SERVER_ISSUED", "username", "password");

        VpnCredentialsIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.ProvisionedConfigText, result.ProvisionedConfigText);
        Assert.AreEqual(entityToTest.Username, result.Username);
        Assert.AreEqual(entityToTest.Password, result.Password);
    }

    [TestMethod]
    public void TestMapRightToLeft_WithProvisionedConfig_NoCertificateRequired()
    {
        VpnCredentialsIpcEntity entityToTest = new()
        {
            Certificate = null,
            ClientKeyPair = null,
            Username = "username",
            Password = "password",
            ProvisionedConfigText = "[Interface]\nPrivateKey = SERVER_ISSUED",
        };

        VpnCredentials result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.ProvisionedConfigText, result.ProvisionedConfigText);
        Assert.AreEqual(entityToTest.Username, result.Username);
        Assert.AreEqual(entityToTest.Password, result.Password);
    }

    [TestMethod]
    public void TestMapLeftToRight_WithPreSharedKey()
    {
        VpnCredentials entityToTest = VpnCredentials.FromRasCredentials("l2tp-user", "l2tp-pass", "l2tp-psk");

        VpnCredentialsIpcEntity result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Username, result.Username);
        Assert.AreEqual(entityToTest.Password, result.Password);
        Assert.AreEqual(entityToTest.PreSharedKey, result.PreSharedKey);
        Assert.IsNull(result.ProvisionedConfigText);
    }

    [TestMethod]
    public void TestMapRightToLeft_WithPreSharedKey_NoKeyPairOrConfigTextRequired()
    {
        VpnCredentialsIpcEntity entityToTest = new()
        {
            Certificate = null,
            ClientKeyPair = null,
            Username = "l2tp-user",
            Password = "l2tp-pass",
            ProvisionedConfigText = null,
            PreSharedKey = "l2tp-psk",
        };

        VpnCredentials result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Username, result.Username);
        Assert.AreEqual(entityToTest.Password, result.Password);
        Assert.AreEqual(entityToTest.PreSharedKey, result.PreSharedKey);
        Assert.IsNull(result.ClientKeyPair);
    }

    [TestMethod]
    public void TestMapRightToLeft_WithNoKeyPairAndNoPreSharedKey_SstpStyleCredentials()
    {
        VpnCredentialsIpcEntity entityToTest = new()
        {
            Certificate = null,
            ClientKeyPair = null,
            Username = "sstp-user",
            Password = "sstp-pass",
            ProvisionedConfigText = null,
            PreSharedKey = null,
        };

        VpnCredentials result = _mapper.Map(entityToTest);

        Assert.IsNotNull(result);
        Assert.AreEqual(entityToTest.Username, result.Username);
        Assert.AreEqual(entityToTest.Password, result.Password);
        Assert.IsNull(result.PreSharedKey);
        Assert.IsNull(result.ClientKeyPair);
    }
}