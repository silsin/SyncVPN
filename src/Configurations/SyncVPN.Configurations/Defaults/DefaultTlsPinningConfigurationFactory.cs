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

using SyncVPN.Configurations.Contracts;
using SyncVPN.Configurations.Contracts.Entities;
using SyncVPN.Configurations.Entities;

namespace SyncVPN.Configurations.Defaults;

public static class DefaultTlsPinningConfigurationFactory
{
    public static ITlsPinningConfiguration Create()
    {
        return new TlsPinningConfiguration()
        {
            Enforce = true,
            PinnedDomains = new List<ITlsPinnedDomain>
            {
                new TlsPinnedDomain()
                {
                    Name = "syncvpn.com",
                    Enforce = true,
                    SendReport = true,
                    PublicKeyHashes = new HashSet<string>
                    {
                        "OHCQhWbRMlsUk7NICIRsTFZQJFHP+fAT4f7SP8afUvg=", // Current leaf (Google Trust Services WE1).
                        "kIdp6NNEd8wsugYyyIYFsi1ylMCED3hZbSR8ZFsa/A4=", // Hot backup - issuing CA (WE1).
                        "mEflZT5enoR1FuXLgYYGqnVEoZvmf9c2bVBpiOjYQ0c=", // Cold backup - root CA (GTS Root R4).
                    },
                },
                new TlsPinnedDomain()
                {
                    Name = "[InternalReleaseHost]", // This is replaced by a CI script
                    Enforce = true,
                    SendReport = true,
                    PublicKeyHashes = new HashSet<string>
                    {
                        "C4SMuz+h4+fTsxOKLXRKqrR9rAzk9bknu+hlC4QYmh0=",
                    },
                },
                new TlsPinnedDomain()
                {
                    Name = Constants.ALTERNATIVE_ROUTING_HOSTNAME,
                    Enforce = true,
                    SendReport = true,
                    PublicKeyHashes = new HashSet<string>
                    {
                        "EU6TS9MO0L/GsDHvVc9D5fChYLNy5JdGYpJw0ccgetM=",
                        "iKPIHPnDNqdkvOnTClQ8zQAIKG0XavaPkcEo0LBAABA=",
                        "MSlVrBCdL0hKyczvgYVSRNm88RicyY04Q2y5qrBt0xA=",
                        "C2UxW0T1Ckl9s+8cXfjXxlEqwAfPM4HiW2y3UdtBeCw=",
                    },
                },
            },
        };
    }
}