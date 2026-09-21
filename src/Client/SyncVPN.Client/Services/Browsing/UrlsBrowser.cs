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

using System.Diagnostics;
using SyncVPN.Client.Contracts.Services.Browsing;
using SyncVPN.Client.Core.Models;
using SyncVPN.Client.Core.Services.Selection;
using SyncVPN.Common.Core.Extensions;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppLogs;

namespace SyncVPN.Client.Services.Browsing;

public class UrlsBrowser : IUrlsBrowser
{
    private readonly IWebBrowserAppSelector _webBrowserAppSelector;
    private readonly ILogger _logger;

    public string ProtocolsLearnMore => "https://syncvpn.com/blog/whats-the-best-vpn-protocol";

    public string CreateAccount => "https://syncvpn.com/pricing";

    public string ResetPassword => "https://syncvpn.com/reset-password";

    public string ForgotUsername => "https://syncvpn.com/forgot-username";

    public string TroubleSigningIn => "https://syncvpn.com/support/login-problems";

    public string ProtocolChangeLearnMore => "https://syncvpn.com/support/how-to-change-vpn-protocols";

    public string ServerLoadLearnMore => "https://syncvpn.com/support/server-load-percentages-and-colors-explained";

    public string InternetSpeedLearnMore => "https://syncvpn.com/support/how-latency-bandwidth-throughput-impact-internet-speed";

    public string NatTypeLearnMore => "https://syncvpn.com/support/moderate-nat";

    public string CustomDnsLearnMore => "https://syncvpn.com/support/custom-dns";

    public string Ipv6LeakProtectionLearnMore => "https://syncvpn.com/support/prevent-ipv6-vpn-leaks";

    public string SupportCenter => "https://syncvpn.com/support";

    public string UsageStatisticsLearnMore => "https://syncvpn.com/support/share-usage-statistics";

    public string NetShieldLearnMore => "https://syncvpn.com/support/netshield";

    public string KillSwitchLearnMore => "https://syncvpn.com/support/what-is-kill-switch";

    public string AdvancedKillSwitchLearnMore => "https://syncvpn.com/support/advanced-kill-switch";

    public string PortForwardingLearnMore => "https://syncvpn.com/support/port-forwarding";

    public string SplitTunnelingLearnMore => "https://syncvpn.com/support/split-tunneling";

    public string VpnAcceleratorLearnMore => "https://syncvpn.com/support/how-to-use-vpn-accelerator";

    public string SecureCoreLearnMore => "https://syncvpn.com/support/secure-core-vpn";

    public string SmartRoutingLearnMore => "https://syncvpn.com/support/how-smart-routing-works";

    public string P2PLearnMore => "https://syncvpn.com/features/p2p-support";

    public string TorLearnMore => "https://syncvpn.com/support/tor-vpn";

    public string RpcServerProblem => "https://syncvpn.com/support/rpc-server-unavailable";

    public string Troubleshooting => "https://syncvpn.com/support/windows-vpn-issues";

    public string NoLogs => "https://syncvpn.com/blog/no-logs-audit";

    public string StatusPage => "https://syncvpn.com/status";

    public string P2PStatusPage => "https://syncvpn.com/vpn_status";

    public string SupportForm => "https://syncvpn.com/support-form";

    public string DownloadsPage => "https://syncvpn.com/download";

    public string IpAddressLearnMore => "https://syncvpn.com/blog/what-is-an-ip-address";

    public string IspLearnMore => "https://syncvpn.com/blog/isp";

    public string IncreaseVpnSpeeds => "https://syncvpn.com/support/increase-vpn-speeds";

    public string ActiveProxyLearnMore => "https://syncvpn.com/support/turn-off-proxy-windows";

    public string EnableBaseFilteringEngine => "https://syncvpn.com/support/how-to-enable-the-base-filtering-engine";

    public string ProfileLearnMore => "https://syncvpn.com/support/connection-profiles";

    public string TrafficLearnMore => "https://syncvpn.com/support/traffic-stats";

    public string TwoFactorAuthLearnMore => "https://syncvpn.com/support/two-factor-authentication-2fa";

    public string LocalDnsLearnMore => "https://syncvpn.com/support/local-dns-devices-by-name";

    public UrlsBrowser(
        IWebBrowserAppSelector webBrowserAppSelector,
        ILogger logger)
    {
        _webBrowserAppSelector = webBrowserAppSelector;
        _logger = logger;
    }

    public void BrowseTo(string url, bool usePrivateBrowsingMode = false)
    {
        if (!url.IsValidUrl())
        {
            _logger.Warn<AppLog>($"Could not navigate to the requested url due to invalid format. Url: {url}");
            return;
        }

        try
        {
            if (usePrivateBrowsingMode)
            {
                WebBrowserApp? defaultWebBrowserApp = _webBrowserAppSelector.GetDefaultWebBrowserApp();
                if (defaultWebBrowserApp != null && defaultWebBrowserApp.SupportsPrivateBrowsing)
                {
                    Process.Start(new ProcessStartInfo 
                    { 
                        FileName = defaultWebBrowserApp.AppPath, 
                        Arguments = $"{defaultWebBrowserApp.PrivateBrowsingArgument} {url}", 
                        UseShellExecute = true 
                    });
                    return;
                }
                else
                {
                    _logger.Info<AppLog>($"Could not navigate to the requested url.  Default browser {(defaultWebBrowserApp == null ? "could not be found" : "does not support private browsing")}.");
                }
                return;
            }

            Process.Start(new ProcessStartInfo 
            { 
                FileName = url, 
                UseShellExecute = true 
            });
        }
        catch (Exception e)
        {
            _logger.Error<AppFileAccessFailedLog>($"Could not navigate to the requested url: {url}", e);
        }
    }
}