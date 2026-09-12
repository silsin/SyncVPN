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

using SyncVPN.Common.Legacy.Vpn;
using SyncVPN.Crypto.Contracts;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Crypto;
using SyncVPN.ProcessCommunication.Contracts.Entities.Vpn;

namespace SyncVPN.ProcessCommunication.EntityMapping.Vpn;

public class VpnCredentialsMapper : IMapper<VpnCredentials, VpnCredentialsIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public VpnCredentialsMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public VpnCredentialsIpcEntity Map(VpnCredentials leftEntity)
    {
        return new()
        {
            Certificate = new()
            {
                Pem = leftEntity.ClientCertPem ?? string.Empty,
                ExpirationDateUtc = leftEntity.ClientCertificateExpirationDateUtc ?? DateTime.MinValue,
            },
            ClientKeyPair = _entityMapper.Map<AsymmetricKeyPair, AsymmetricKeyPairIpcEntity>(leftEntity.ClientKeyPair),
            Username = leftEntity.Username,
            Password = leftEntity.Password,
            ProvisionedConfigText = leftEntity.ProvisionedConfigText,
            PreSharedKey = leftEntity.PreSharedKey,
        };
    }

    public VpnCredentials Map(VpnCredentialsIpcEntity rightEntity)
    {
        if (rightEntity.ProvisionedConfigText is not null)
        {
            return VpnCredentials.FromProvisionedConfig(rightEntity.ProvisionedConfigText, rightEntity.Username, rightEntity.Password);
        }

        // L2TP/SSTP credentials carry neither a provisioned config nor a client keypair/certificate -
        // this combination was never valid before those protocols existed, so its absence here is a
        // reliable signal to build RAS-based credentials instead of the legacy cert/keypair ones below.
        if (rightEntity.ClientKeyPair is null)
        {
            return VpnCredentials.FromRasCredentials(rightEntity.Username, rightEntity.Password, rightEntity.PreSharedKey);
        }

        return new(rightEntity.Certificate?.Pem,
            rightEntity.Certificate?.ExpirationDateUtc,
            _entityMapper.Map<AsymmetricKeyPairIpcEntity, AsymmetricKeyPair>(rightEntity.ClientKeyPair),
            rightEntity.Username,
            rightEntity.Password);
    }
}