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

using SyncVPN.IssueReporting.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.OperatingSystemLogs;
using SyncVPN.OperatingSystems.NRPT.Contracts;

namespace SyncVPN.OperatingSystems.NRPT;

public class NrptInvoker : INrptInvoker
{
    private readonly ILogger _logger;
    private readonly IIssueReporter _issueReporter;

    public NrptInvoker(ILogger logger, IIssueReporter issueReporter)
    {
        _logger = logger;
        _issueReporter = issueReporter;
    }

    /// <returns>If the NRPT rule was added successfully</returns>
    public bool CreateRule(string nameServers)
    {
        if (string.IsNullOrWhiteSpace(nameServers))
        {
            _logger.Error<OperatingSystemNrptLog>("No DNS servers when creating NRPT rule. No NRPT rule will be created.");
            return false;
        }

        return StaticNrptInvoker.CreateRule(nameServers, OnException, OnCreateError, OnSuccess);
    }

    private void OnException(string errorMessage, Exception ex)
    {
        _logger.Error<OperatingSystemNrptLog>(errorMessage, ex);
        _issueReporter.CaptureError(new Exception(errorMessage, ex));
    }

    private void OnCreateError(string errorMessage)
    {
        _logger.Error<OperatingSystemNrptLog>(errorMessage);
        _issueReporter.CaptureError("Error when creating NRPT rule", errorMessage);
    }

    private void OnSuccess(string message)
    {
        _logger.Info<OperatingSystemNrptLog>(message);
    }

    /// <returns>If the NRPT rule was removed successfully</returns>
    public bool DeleteRule()
    {
        return StaticNrptInvoker.DeleteRule(OnException, OnSuccess);
    }
}
