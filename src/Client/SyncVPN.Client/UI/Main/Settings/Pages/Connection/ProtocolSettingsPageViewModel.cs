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

using CommunityToolkit.Mvvm.ComponentModel;
using SyncVPN.Client.Common.Attributes;
using SyncVPN.Client.Contracts.Services.Browsing;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.RequestCreators;
using SyncVPN.Client.Services.FreeServers;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.RequiredReconnections;
using SyncVPN.Client.UI.Main.Settings.Bases;
using SyncVPN.Common.Core.Networking;

namespace SyncVPN.Client.UI.Main.Settings.Pages.Connection;

public partial class ProtocolSettingsPageViewModel : SettingsPageViewModelBase
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IFreeServersCache _freeServersCache;

    [ObservableProperty]
    [property: SettingName(nameof(ISettings.VpnProtocol))]
    [NotifyPropertyChangedFor(nameof(IsSmartProtocol))]
    [NotifyPropertyChangedFor(nameof(IsWireGuardUdpProtocol))]
    [NotifyPropertyChangedFor(nameof(IsWireGuardTcpProtocol))]
    [NotifyPropertyChangedFor(nameof(IsWireGuardTlsProtocol))]
    [NotifyPropertyChangedFor(nameof(IsOpenVpnUdpProtocol))]
    [NotifyPropertyChangedFor(nameof(IsOpenVpnTcpProtocol))]
    [NotifyPropertyChangedFor(nameof(IsL2tpProtocol))]
    [NotifyPropertyChangedFor(nameof(IsSstpProtocol))]
    private VpnProtocol _currentVpnProtocol;

    public override string Title => Localizer.Get("Settings_Connection_Protocol");

    public string Recommended => Localizer.Get("Common_Tags_Recommended").ToUpperInvariant();

    public bool IsSmartProtocol
    {
        get => IsProtocol(VpnProtocol.Smart);
        set => SetProtocol(value, VpnProtocol.Smart);
    }

    public bool IsWireGuardUdpProtocol
    {
        get => IsProtocol(VpnProtocol.WireGuardUdp);
        set => SetProtocol(value, VpnProtocol.WireGuardUdp);
    }

    public bool IsWireGuardTcpProtocol
    {
        get => IsProtocol(VpnProtocol.WireGuardTcp);
        set => SetProtocol(value, VpnProtocol.WireGuardTcp);
    }

    public bool IsWireGuardTlsProtocol
    {
        get => IsProtocol(VpnProtocol.WireGuardTls);
        set => SetProtocol(value, VpnProtocol.WireGuardTls);
    }

    public bool IsOpenVpnUdpProtocol
    {
        get => IsProtocol(VpnProtocol.OpenVpnUdp);
        set => SetProtocol(value, VpnProtocol.OpenVpnUdp);
    }

    public bool IsOpenVpnTcpProtocol
    {
        get => IsProtocol(VpnProtocol.OpenVpnTcp);
        set => SetProtocol(value, VpnProtocol.OpenVpnTcp);
    }

    public bool IsL2tpProtocol
    {
        get => IsProtocol(VpnProtocol.L2tp);
        set => SetProtocol(value, VpnProtocol.L2tp);
    }

    public bool IsSstpProtocol
    {
        get => IsProtocol(VpnProtocol.Sstp);
        set => SetProtocol(value, VpnProtocol.Sstp);
    }

    // Smart is a client-side mode, not a wire protocol - always offered. Every other option is hidden
    // unless at least one cached server actually advertises it (see IFreeServersCache.IsProtocolAvailable,
    // which fails open while the catalog hasn't loaded yet, so this never renders an empty page on a
    // cold start). WireGuardTcp/WireGuardTls have no wire protocol at all on this backend, so they're
    // never available by this rule.
    public bool IsWireGuardUdpProtocolAvailable => IsProtocolAvailable(VpnProtocol.WireGuardUdp);
    public bool IsWireGuardTcpProtocolAvailable => IsProtocolAvailable(VpnProtocol.WireGuardTcp);
    public bool IsWireGuardTlsProtocolAvailable => IsProtocolAvailable(VpnProtocol.WireGuardTls);
    public bool IsOpenVpnUdpProtocolAvailable => IsProtocolAvailable(VpnProtocol.OpenVpnUdp);
    public bool IsOpenVpnTcpProtocolAvailable => IsProtocolAvailable(VpnProtocol.OpenVpnTcp);
    public bool IsL2tpProtocolAvailable => IsProtocolAvailable(VpnProtocol.L2tp);
    public bool IsSstpProtocolAvailable => IsProtocolAvailable(VpnProtocol.Sstp);

    public bool IsUdpCategoryVisible => IsWireGuardUdpProtocolAvailable || IsOpenVpnUdpProtocolAvailable;
    public bool IsTcpCategoryVisible => IsWireGuardTcpProtocolAvailable || IsOpenVpnTcpProtocolAvailable || IsWireGuardTlsProtocolAvailable;
    public bool IsLegacyCategoryVisible => IsL2tpProtocolAvailable || IsSstpProtocolAvailable;

    public string LearnMoreUrl => _urlsBrowser.ProtocolsLearnMore;

    public ProtocolSettingsPageViewModel(
        IUrlsBrowser urlsBrowser,
        IRequiredReconnectionSettings requiredReconnectionSettings,
        IMainViewNavigator mainViewNavigator,
        ISettingsViewNavigator settingsViewNavigator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        ISettings settings,
        ISettingsConflictResolver settingsConflictResolver,
        IConnectionManager connectionManager,
        IFreeServersCache freeServersCache,
        IViewModelHelper viewModelHelper)
        : base(requiredReconnectionSettings,
               mainViewNavigator,
               settingsViewNavigator,
               mainWindowOverlayActivator,
               settings,
               settingsConflictResolver,
               connectionManager,
               viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _freeServersCache = freeServersCache;

        PageSettings =
        [
            ChangedSettingArgs.Create(() => Settings.VpnProtocol, () => CurrentVpnProtocol)
        ];
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Recommended));
    }

    protected override void OnRetrieveSettings()
    {
        CurrentVpnProtocol = Settings.VpnProtocol;

        // A previously-picked protocol may no longer be offered by any cached server (catalog changed
        // since this was last set) - fall back to Smart rather than leaving a hidden option selected.
        if (CurrentVpnProtocol != VpnProtocol.Smart && !IsProtocolAvailable(CurrentVpnProtocol))
        {
            CurrentVpnProtocol = VpnProtocol.Smart;
        }
    }

    private bool IsProtocol(VpnProtocol protocol)
    {
        return CurrentVpnProtocol == protocol;
    }

    private void SetProtocol(bool value, VpnProtocol protocol)
    {
        if (value)
        {
            CurrentVpnProtocol = protocol;
        }
    }

    private bool IsProtocolAvailable(VpnProtocol protocol)
    {
        string? wireProtocol = SyncVpnAccountClaimMapper.MapToWireProtocol(protocol);
        return wireProtocol is not null && _freeServersCache.IsProtocolAvailable(wireProtocol);
    }
}