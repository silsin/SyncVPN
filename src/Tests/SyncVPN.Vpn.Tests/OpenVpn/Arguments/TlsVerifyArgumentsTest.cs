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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SyncVPN.Configurations.Contracts.Entities;
using SyncVPN.Vpn.OpenVpn.Arguments;

namespace SyncVPN.Vpn.Tests.OpenVpn.Arguments;

[TestClass]
public class TlsVerifyArgumentsTest
{
    private IOpenVpnConfigurations _openVpnConfig;

    [TestInitialize]
    public void TestInitialize()
    {
        _openVpnConfig = Substitute.For<IOpenVpnConfigurations>();
    }

    [TestMethod]
    public void Enumerable_ShouldContain_ExpectedNumberOfOptions()
    {
        // Arrange
        _openVpnConfig.TlsExportCertFolder.Returns("ExportCert");
        _openVpnConfig.TlsVerifyExePath.Returns("SyncVPN.TlsVerify.exe");
        TlsVerifyArguments subject = new(_openVpnConfig, "nl-101.syncvpn.com");

        // Act
        List<string> result = subject.ToList();

        // Assert
        result.Should().HaveCount(3);
    }

    [TestMethod]
    public void Enumerable_ShouldContain_SetEnvOption()
    {
        // Arrange
        const string serverName = "nl-1.syncvpn.com";

        _openVpnConfig.TlsExportCertFolder.Returns("ExportCert");
        _openVpnConfig.TlsVerifyExePath.Returns("SyncVPN.TlsVerify.exe");
        TlsVerifyArguments subject = new(_openVpnConfig, serverName);

        // Act
        List<string> result = subject.ToList();

        // Assert
        result.Should().Contain($"--setenv peer_dns_name \"{serverName}\"");
    }

    [TestMethod]
    public void Enumerable_ShouldContain_TlsExportCertOption()
    {
        // Arrange
        const string exportCertFolder = "C:\\ProgramData\\ExportCert";

        _openVpnConfig.TlsExportCertFolder.Returns(exportCertFolder);
        _openVpnConfig.TlsVerifyExePath.Returns("SyncVPN.TlsVerify.exe");
        TlsVerifyArguments subject = new(_openVpnConfig, "gb-15.syncvpn.com");

        // Act
        List<string> result = subject.ToList();

        // Assert
        result.Should().Contain($"--tls-export-cert \"{exportCertFolder}\"");
    }

    [TestMethod]
    public void Enumerable_ShouldContain_TlsVerifyOption()
    {
        // Arrange
        const string tlsVerifyExePath = "C:\\Program Files\\TlsVerify.exe";

        _openVpnConfig.TlsExportCertFolder.Returns("ExportCert");
        _openVpnConfig.TlsVerifyExePath.Returns(tlsVerifyExePath);
        TlsVerifyArguments subject = new(_openVpnConfig, "gb-15.syncvpn.com");

        // Act
        List<string> result = subject.ToList();

        // Assert
        result.Should().Contain($"--tls-verify \"{tlsVerifyExePath}\"");
    }
}
