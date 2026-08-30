/*
 * Copyright (c) 2024 Proton AG
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

using SyncVPN.Api.Contracts.Announcements;
using SyncVPN.Client.Files.Contracts.Images;
using SyncVPN.Client.Logic.Announcements.Contracts;
using SyncVPN.Client.Logic.Announcements.Contracts.Entities;
using SyncVPN.EntityMapping.Contracts;

namespace SyncVPN.Client.Logic.Announcements.EntityMapping;

public class PanelFeatureMapper : IMapper<OfferPanelFeatureResponse, PanelFeature>
{
    private readonly IImageCache _imageCache;

    public PanelFeatureMapper(IImageCache imageCache)
    {
        _imageCache = imageCache;
    }

    public PanelFeature Map(OfferPanelFeatureResponse leftEntity)
    {
        return new()
        {
            Icon = _imageCache.Get(AnnouncementConstants.STORAGE_FOLDER, leftEntity.IconUrl),
            Text = leftEntity.Text
        };
    }

    public OfferPanelFeatureResponse Map(PanelFeature rightEntity)
    {
        throw new NotImplementedException("We don't need to map to API responses.");
    }
}