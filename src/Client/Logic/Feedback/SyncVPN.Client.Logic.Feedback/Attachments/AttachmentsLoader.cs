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

using SyncVPN.Client.Logic.Feedback.Attachments.Sources;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;

namespace SyncVPN.Client.Logic.Feedback.Attachments;

public class AttachmentsLoader : IAttachmentsLoader
{
    private readonly ILogger _logger;
    private readonly IDiagnosticsLogFileSource _diagnosticsLogFileSource;
    private readonly IClientLogFileSource _appLogFileSource;
    private readonly IServiceLogFileSource _serviceLogFileSource;

    public AttachmentsLoader(ILogger logger, IDiagnosticsLogFileSource diagnosticsLogFileSource,
        IClientLogFileSource appLogFileSource, IServiceLogFileSource serviceLogFileSource)
    {
        _logger = logger;
        _diagnosticsLogFileSource = diagnosticsLogFileSource;
        _appLogFileSource = appLogFileSource;
        _serviceLogFileSource = serviceLogFileSource;
    }

    public IList<Attachment> Get()
    {
        try
        {
            return GetFromFileSources()
                   .SelectMany(i => i)
                   .Select(filename => new Attachment(filename))
                   .ToList();
        }
        catch (Exception e)
        {
            _logger.Error<AppLog>("An unexpected error occurred when fetching and processing the attachments.", e);
            return new List<Attachment>();
        }
    }

    private IList<IEnumerable<string>> GetFromFileSources()
    {
        return new List<IEnumerable<string>>
            {
                _diagnosticsLogFileSource.Get(),
                _appLogFileSource.Get(),
                _serviceLogFileSource.Get()
            };
    }
}
