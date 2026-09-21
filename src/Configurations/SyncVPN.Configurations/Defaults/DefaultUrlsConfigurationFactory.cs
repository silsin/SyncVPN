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

using SyncVPN.Configurations.Contracts.Entities;
using SyncVPN.Configurations.Entities;

namespace SyncVPN.Configurations.Defaults;

public static class DefaultUrlsConfigurationFactory
{
    public static IUrlsConfiguration Create()
    {
        return new UrlsConfiguration()
        {
            ApiUrl = "https://syncvpn.com/api/",
            SyncVpnApiUrl = "https://syncvpn.com/api/",
            BfeArticleUrl = "https://syncvpn.com/support/how-to-enable-the-base-filtering-engine",
            PasswordResetUrl = "https://syncvpn.com/reset-password",
            ForgetUsernameUrl = "https://syncvpn.com/forgot-username",
            UpdateUrl = "https://syncvpn.com/download/windows/{0}/v1/version.json",
            DownloadUrl = "https://syncvpn.com/download",
            TlsReportUrl = "https://syncvpn.com/reports/tls",
            HelpUrl = "https://syncvpn.com/support/",
            AutoLoginBaseUrl = "https://syncvpn.com/lite",
            AccountUrl = "https://syncvpn.com/account",
            AboutSecureCoreUrl = "https://syncvpn.com/support/secure-core-vpn",
            RegisterUrl = "https://syncvpn.com/signup",
            TroubleShootingUrl = "https://syncvpn.com/support/windows-vpn-issues",
            P2PStatusUrl = "https://syncvpn.com/vpn_status",
            MailPricingUrl = "https://syncvpn.com/pricing",
            PublicWifiSafetyUrl = "https://syncvpn.com/blog/public-wifi-safety",
            StatusUrl = "https://syncvpn.com/status",
            TorBrowserUrl = "https://www.torproject.org",
            TwitterUrl = "https://twitter.com/SyncVPN",
            SupportFormUrl = "https://syncvpn.com/support-form",
            AlternativeRoutingUrl = "https://syncvpn.com/blog/anti-censorship-alternative-routing",
            AboutKillSwitchUrl = "https://syncvpn.com/support/what-is-kill-switch",
            AboutNetShieldUrl = "https://syncvpn.com/support/netshield",
            AboutPortForwardingUrl = "https://syncvpn.com/support/port-forwarding",
            PortForwardingRisksUrl = "https://syncvpn.com/support/port-forwarding-risks",
            AboutModerateNatUrl = "https://syncvpn.com/support/moderate-nat",
            StreamingUrl = "https://syncvpn.com/support/streaming-guide/",
            SmartRoutingUrl = "https://syncvpn.com/support/smart-routing",
            P2PUrl = "https://syncvpn.com/support/bittorrent-vpn/",
            TorUrl = "https://syncvpn.com/support/tor-vpn/",
            InvoicesUrl = "https://syncvpn.com/payments#invoices",
            AboutSmartProtocolUrl = "https://syncvpn.com/support/how-to-change-vpn-protocols",
            IncorrectSystemTimeArticleUrl = "https://syncvpn.com/support/update-windows-clock",
            EnableVpnConnectionsUrl = "https://syncvpn.com/support/enable-vpn-connection",
            LoginProblemsUrl = "https://syncvpn.com/support/login-problems",
            RebrandingUrl = "https://syncvpn.com/blog/updated-sync-vpn",
            RpcServerProblemUrl = "https://syncvpn.com/support/rpc-server-unavailable",
        };
    }
}