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

using SyncVPN.Client.Contracts.Services.Validation;
using SyncVPN.Client.Core.Messages;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.OperatingSystemLogs;
using SyncVPN.NetworkTimeProtocols.Contracts;

namespace SyncVPN.Client.Services.Validation;

public class SystemTimeValidator : ISystemTimeValidator
{
    private const int MAX_DIFFERENCE_IN_SECONDS = 60;

    private readonly INtpClient _ntpClient;
    private readonly ILogger _logger;
    private readonly IEventMessageSender _eventMessageSender;

    public SystemTimeValidator(INtpClient ntpClient, ILogger logger,
        IEventMessageSender eventMessageSender)
    {
        _ntpClient = ntpClient;
        _logger = logger;
        _eventMessageSender = eventMessageSender;
    }

    public async Task CheckAsync(CancellationToken cancellationToken)
    {
        DateTime? networkTime = await _ntpClient.GetNetworkUtcTimeAsync(cancellationToken);
        DateTime utcNow = DateTime.UtcNow;

        if (networkTime.HasValue &&
            Math.Abs((networkTime.Value - utcNow).TotalSeconds) > MAX_DIFFERENCE_IN_SECONDS)
        {
            _logger.Warn<OperatingSystemLog>($"Incorrect system time detected " +
                $"[Network time: {networkTime}] [System time: {utcNow}].");

            _eventMessageSender.Send(new IncorrectSystemTimeMessage());
        }
    }
}
