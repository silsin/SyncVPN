/*
 * Copyright (c) 2026 Proton AG
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

using SyncVPN.Api.Contracts;
using SyncVPN.Api.V2.Contracts.Servers;
using SyncVPN.Client.Common.Observers;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Purchases.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.IssueReporting.Contracts;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.ApiLogs;

namespace SyncVPN.Client.Services.FreeServers;

// Fetches the new SyncVPN backend's server catalog and caches it in IFreeServersCache for the
// Countries sidebar list (see SyncVpnServerLocationItem). A paid account only ever sees the Pro
// catalog (GET /servers/pro) - once logged into a Pro plan, the free list has no reason to still be
// shown alongside it, so the two are mutually exclusive rather than merged. A free/guest device only
// ever sees the free catalog (GET /servers), since /servers/pro requires a device token and rejects
// free/anonymous callers. Re-triggers on VpnPlanChangedMessage so logging into (or out of) a paid plan
// swaps the catalog immediately instead of only taking effect on the next app start.
public class FreeServersObserver : ObserverBase, IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IFreeServersProvider _freeServersProvider;
    private readonly IFreeServersCache _freeServersCache;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;

    public FreeServersObserver(
        ILogger logger,
        IIssueReporter issueReporter,
        IFreeServersProvider freeServersProvider,
        IFreeServersCache freeServersCache,
        ISettings settings,
        IEventMessageSender eventMessageSender)
        : base(logger, issueReporter)
    {
        _freeServersProvider = freeServersProvider;
        _freeServersCache = freeServersCache;
        _settings = settings;
        _eventMessageSender = eventMessageSender;

        TriggerAction.Run();
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        TriggerAction.Run();
    }

    protected override async Task OnTriggerAsync()
    {
        ApiResponseResult<ServerListResponse> response = _settings.VpnPlan.IsPaid
            ? await _freeServersProvider.GetProServersAsync()
            : await _freeServersProvider.GetFreeServersAsync();

        if (!response.Success || response.Value is null)
        {
            Logger.Warn<ApiLog>($"Failed to fetch the {(_settings.VpnPlan.IsPaid ? "Pro" : "free")}-server catalog: {response.Error}");
            return;
        }

        _freeServersCache.SetServers(response.Value.Data);

        // Reuses the legacy servers-cache signal - the Countries sidebar already refetches its items on
        // this message, and this catalog is functionally part of "the server list" it renders.
        _eventMessageSender.Send(new ServerListChangedMessage());
    }
}
