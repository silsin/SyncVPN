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


using SyncVPN.Common.Legacy.Restrictions;
using SyncVPN.EntityMapping.Contracts;
using SyncVPN.ProcessCommunication.Contracts.Entities.Restrictions;

namespace SyncVPN.ProcessCommunication.EntityMapping.Restrictions;

public class RestrictionListMapper : IMapper<RestrictionsList, RestrictionsIpcEntity>
{
    private readonly IEntityMapper _entityMapper;

    public RestrictionListMapper(IEntityMapper entityMapper)
    {
        _entityMapper = entityMapper;
    }

    public RestrictionsIpcEntity Map(RestrictionsList leftEntity)
    {
        return new()
        {
            Restrictions = _entityMapper.Map<Restriction, RestrictionIpcEntity>(leftEntity.Restrictions)
        };
    }

    public RestrictionsList Map(RestrictionsIpcEntity rightEntity)
    {
        return new()
        {
            Restrictions = _entityMapper.Map<RestrictionIpcEntity, Restriction>(rightEntity.Restrictions)
        };
    }
}