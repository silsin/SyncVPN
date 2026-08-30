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

using SyncVPN.Common.Legacy.NetShield;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.NetShield;

namespace SyncVPN.ProcessCommunication.EntityMapping.NetShield;

public class NetShieldStatisticMapper : IMapper<NetShieldStatistic, NetShieldStatisticIpcEntity>
{
    public NetShieldStatisticIpcEntity Map(NetShieldStatistic leftEntity)
    {
        return leftEntity is null
            ? null
            : new NetShieldStatisticIpcEntity()
            {
                NumOfMaliciousUrlsBlocked = leftEntity.NumOfMaliciousUrlsBlocked,
                NumOfAdvertisementUrlsBlocked = leftEntity.NumOfAdvertisementUrlsBlocked,
                NumOfTrackingUrlsBlocked = leftEntity.NumOfTrackingUrlsBlocked,
                NumOfAdultContentUrlsBlocked = leftEntity.NumOfAdultContentUrlsBlocked,
                TimestampUtc = leftEntity.TimestampUtc,
            };
    }

    public NetShieldStatistic Map(NetShieldStatisticIpcEntity rightEntity)
    {
        return rightEntity is null
            ? null
            : new NetShieldStatistic()
            {
                NumOfMaliciousUrlsBlocked = rightEntity.NumOfMaliciousUrlsBlocked,
                NumOfAdvertisementUrlsBlocked = rightEntity.NumOfAdvertisementUrlsBlocked,
                NumOfTrackingUrlsBlocked = rightEntity.NumOfTrackingUrlsBlocked,
                NumOfAdultContentUrlsBlocked = rightEntity.NumOfAdultContentUrlsBlocked,
                TimestampUtc = rightEntity.TimestampUtc,
            };
    }
}