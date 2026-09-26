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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SyncVPN.Client.Common.Enums;
using SyncVPN.Client.Common.Models;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Core.Services.Navigation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts;
using SyncVPN.Client.Logic.Connection.Contracts.Enums;
using SyncVPN.Client.Logic.Connection.Contracts.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Logic.Connection.Contracts.Models;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents;
using SyncVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using SyncVPN.Client.Logic.Profiles.Contracts.Messages;
using SyncVPN.Client.Logic.Profiles.Contracts.Models;
using SyncVPN.Client.Logic.Recents.Contracts;
using SyncVPN.Client.Logic.Recents.Contracts.Messages;
using SyncVPN.Client.Logic.Servers.Contracts;
using SyncVPN.Client.Logic.Servers.Contracts.Enums;
using SyncVPN.Client.Logic.Servers.Contracts.Extensions;
using SyncVPN.Client.Logic.Servers.Contracts.Messages;
using SyncVPN.Client.Logic.Services.Contracts;
using SyncVPN.Client.Logic.Services.Contracts.Messages;
using SyncVPN.Client.Logic.Users.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.StatisticalEvents.Contracts.Dimensions;

namespace SyncVPN.Client.UI.Main.Home.Card;

public partial class ConnectionCardComponentViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<ConnectionStatusChangedMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<RecentConnectionsChangedMessage>,
    IEventMessageReceiver<ProfilesChangedMessage>,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<ServerListChangedMessage>,
    IEventMessageReceiver<LocationNamesChangedMessage>,
    IEventMessageReceiver<ServiceEnablementChangedMessage>
{
    private const int FREE_COUNTRIES_DISPLAYED_AS_FLAGS = 3;

    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;
    private readonly IRecentConnectionsManager _recentConnectionsManager;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IServersLoader _serversLoader;
    private readonly IServiceManager _serviceManager;
    private readonly IMainViewNavigator _mainViewNavigator;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelConnectionCommand))]
    [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowFreeConnectionsOverlayCommand))]
    [NotifyPropertyChangedFor(nameof(Profile))]
    [NotifyPropertyChangedFor(nameof(IsProfileIntent))]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitleOrFeature))]
    [NotifyPropertyChangedFor(nameof(IsDisconnected))]
    [NotifyPropertyChangedFor(nameof(IsConnecting))]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    [NotifyPropertyChangedFor(nameof(IsConnectingViaThisButton))]
    [NotifyPropertyChangedFor(nameof(CanSelectServer))]
    [NotifyPropertyChangedFor(nameof(ShowMapConnectCard))]
    [NotifyPropertyChangedFor(nameof(ShowConnectButton))]
    [NotifyPropertyChangedFor(nameof(ShowCancelButton))]
    [NotifyPropertyChangedFor(nameof(ShowEnableServiceButton))]
    [NotifyCanExecuteChangedFor(nameof(EnableServiceCommand))]
    [NotifyPropertyChangedFor(nameof(IsFreeConnectionsTaglineVisible))]
    [NotifyPropertyChangedFor(nameof(IsChangeServerOptionVisible))]
    [NotifyPropertyChangedFor(nameof(ExitCountry))]
    [NotifyPropertyChangedFor(nameof(EntryCountry))]
    [NotifyPropertyChangedFor(nameof(IsSecureCore))]
    [NotifyPropertyChangedFor(nameof(IsTor))]
    [NotifyPropertyChangedFor(nameof(IsP2P))]
    [NotifyPropertyChangedFor(nameof(IsB2B))]
    [NotifyPropertyChangedFor(nameof(FlagType))]
    private ConnectionStatus _currentConnectionStatus;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Profile))]
    [NotifyPropertyChangedFor(nameof(IsProfileIntent))]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitleOrFeature))]
    [NotifyPropertyChangedFor(nameof(ExitCountry))]
    [NotifyPropertyChangedFor(nameof(EntryCountry))]
    [NotifyPropertyChangedFor(nameof(IsSecureCore))]
    [NotifyPropertyChangedFor(nameof(IsTor))]
    [NotifyPropertyChangedFor(nameof(IsP2P))]
    [NotifyPropertyChangedFor(nameof(IsB2B))]
    [NotifyPropertyChangedFor(nameof(FlagType))]
    private IConnectionIntent? _currentConnectionIntent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Profile))]
    [NotifyPropertyChangedFor(nameof(IsProfileIntent))]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitle))]
    [NotifyPropertyChangedFor(nameof(HasSubtitleOrFeature))]
    [NotifyPropertyChangedFor(nameof(ExitCountry))]
    [NotifyPropertyChangedFor(nameof(EntryCountry))]
    [NotifyPropertyChangedFor(nameof(IsSecureCore))]
    [NotifyPropertyChangedFor(nameof(IsTor))]
    [NotifyPropertyChangedFor(nameof(IsP2P))]
    [NotifyPropertyChangedFor(nameof(IsB2B))]
    [NotifyPropertyChangedFor(nameof(FlagType))]
    private ConnectionDetails? _currentConnectionDetails;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedFreeCountriesCount))]
    private int _freeCountriesCount;

    public string FormattedFreeCountriesCount => FreeCountriesCount > FREE_COUNTRIES_DISPLAYED_AS_FLAGS
        ? $"+{FreeCountriesCount - FREE_COUNTRIES_DISPLAYED_AS_FLAGS}"
        : string.Empty;

    public IConnectionProfile? Profile => CurrentConnectionStatus switch
    {
        ConnectionStatus.Connected => CurrentConnectionDetails?.OriginalConnectionIntent as IConnectionProfile,
        _ => CurrentConnectionIntent as IConnectionProfile
    };

    public bool IsProfileIntent => Profile != null;

    public string Title => GetConnectionCardTitle();

    public string Subtitle => GetConnectionCardSubtitle();

    public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);

    public bool HasSubtitleOrFeature => HasSubtitle || IsTor || IsP2P;

    public bool IsDisconnected => CurrentConnectionStatus == ConnectionStatus.Disconnected;

    public bool IsConnecting => CurrentConnectionStatus == ConnectionStatus.Connecting;

    public bool IsConnected => CurrentConnectionStatus == ConnectionStatus.Connected;

    // True only while the in-progress connection was actually started via THIS button (ConnectCommand
    // below), as opposed to the Map's own MapSelectedConnectButton - lets this button stay visible but
    // disabled instead of disappearing when it's the one the user pressed, while the other surface
    // keeps offering Cancel. See MapComponentViewModel.IsConnectingFromMapSelection for the mirror.
    public bool IsConnectingViaThisButton => IsConnecting && _connectionManager.CurrentConnectionTrigger == VpnTriggerDimension.ConnectionCard;

    // "Actually disconnected AND the service can accept a connection" - also used by the map's "Please
    // select a server" prompt (see HomeComponentView.xaml), which is equally meaningless while the
    // service is down: there's nothing useful to select a server for until Enable resolves it.
    public bool CanSelectServer => IsDisconnected && _serviceManager.IsServiceEnabled;

    // The map's whole floating "glass" connect card (HomeComponentView.xaml's MapConnectButtonContainer)
    // needs its OWN visibility gate, separate from CanSelectServer/ShowConnectButton - otherwise, once
    // the service is down and everything inside it (the "select a server" text, the buttons) hides, the
    // translucent panel itself was still rendering as an empty box floating over the map. Stays visible
    // whenever there's still something meaningful inside it (connecting/connected), only disappears for
    // the specific "disconnected and can't do anything about it yet" case.
    public bool ShowMapConnectCard => !IsDisconnected || _serviceManager.IsServiceEnabled;

    // Hidden (not merely disabled) while the service is unavailable - ShowEnableServiceButton takes its
    // place in the button group instead, so there's always exactly one clear, actionable button rather
    // than a Connect button that looks pressable but silently does nothing.
    public bool ShowConnectButton => CanSelectServer || IsConnectingViaThisButton;

    public bool ShowCancelButton => IsConnecting && !IsConnectingViaThisButton;

    public bool ShowEnableServiceButton => IsDisconnected && !_serviceManager.IsServiceEnabled;

    // Shown under the Connect/Cancel button group while IsConnecting - a connection attempt can sit at
    // "trying to reach the server" for tens of seconds (see VpnEndpointScanner/ConnectingWatchdogTimeout)
    // with no other visible feedback that anything is happening beyond a generic spinner.
    public string ConnectingStatusText => !IsConnecting
        ? string.Empty
        : _connectionManager.IsPinging
            ? Localizer.Get("Home_ConnectionCard_Status_TryingToReachServer")
            : Localizer.Get("Home_ConnectionCard_Status_Connecting");

    public bool IsFreeUser => !_settings.VpnPlan.IsPaid;

    public bool IsFreeConnectionsTaglineVisible => IsFreeUser && !IsConnected;

    public bool IsChangeServerOptionVisible => IsFreeUser && IsConnected;

    public string? ExitCountry =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected => CurrentConnectionDetails?.ExitCountryCode,
            _ => CurrentConnectionIntent?.Location?.GetCountryCode()
        };

    public string? EntryCountry =>
        CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected =>
                CurrentConnectionDetails?.OriginalConnectionIntent.Feature is SecureCoreFeatureIntent secureCoreIntent && !secureCoreIntent.IsFastest
                    ? CurrentConnectionDetails.EntryCountryCode
                    : string.Empty,
            _ => (CurrentConnectionIntent?.Feature as SecureCoreFeatureIntent)?.EntryCountryCode
        };

    public bool IsSecureCore => IsFeature<SecureCoreFeatureIntent>(ServerFeatures.SecureCore);

    public bool IsTor => IsFeature<TorFeatureIntent>(ServerFeatures.Tor);

    public bool IsP2P => IsFeature<P2PFeatureIntent>(ServerFeatures.P2P);

    public bool IsB2B => IsFeature<B2BFeatureIntent>(ServerFeatures.B2B);

    public FlagType FlagType => (CurrentConnectionStatus switch
    {
        ConnectionStatus.Connected => CurrentConnectionDetails?.OriginalConnectionIntent.Location,
        _ => CurrentConnectionIntent?.Location
    }).GetFlagType(CurrentConnectionStatus is ConnectionStatus.Connected);

    public ConnectionCardComponentViewModel(
        IViewModelHelper viewModelHelper,
        IConnectionManager connectionManager,
        ISettings settings,
        IRecentConnectionsManager recentConnectionsManager,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IServersLoader serversLoader,
        IServiceManager serviceManager,
        IMainViewNavigator mainViewNavigator)
        : base(viewModelHelper)
    {
        _connectionManager = connectionManager;
        _settings = settings;
        _recentConnectionsManager = recentConnectionsManager;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _serversLoader = serversLoader;
        _serviceManager = serviceManager;
        _mainViewNavigator = mainViewNavigator;
    }

    [RelayCommand]
    private Task OpenCountriesAsync()
    {
        return _mainViewNavigator.NavigateToCountriesViewAsync();
    }

    public void Receive(ServiceEnablementChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(() =>
            {
                ConnectCommand.NotifyCanExecuteChanged();
                EnableServiceCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(CanSelectServer));
                OnPropertyChanged(nameof(ShowMapConnectCard));
                OnPropertyChanged(nameof(ShowConnectButton));
                OnPropertyChanged(nameof(ShowEnableServiceButton));
            });
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        ExecuteOnUIThread(() =>
        {
            if (IsActive)
            {
                InvalidateConnectionStatus();

                // Not covered by CurrentConnectionStatus's own change notification - that's the 3-value
                // ConnectionStatus bucket (Disconnected/Connecting/Connected), which stays "Connecting"
                // across a Connecting -> Pinging -> Connecting transition even though IsPinging (read live
                // off _connectionManager, not off this message) did change and the status text needs to.
                OnPropertyChanged(nameof(ConnectingStatusText));

                if (message.ConnectionStatus == ConnectionStatus.Connected)
                {
                    InvalidateConnectionDetails();
                }
                else
                {
                    InvalidateConnectionIntent();
                }
            }
        });
    }

    public void Receive(ServerListChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(() =>
            {
                InvalidateFreeCountriesCount();
                InvalidateConnectionIntent();
            });
        }
    }

    public void Receive(LocationNamesChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(() => OnPropertyChanged(nameof(Subtitle)));
        }
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateVpnPlan);
        }
    }

    public void Receive(RecentConnectionsChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateConnectionIntent);
        }
    }

    public void Receive(ProfilesChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(InvalidateConnectionIntent);
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (IsActive && message.PropertyName == nameof(ISettings.DefaultConnection))
        {
            ExecuteOnUIThread(InvalidateConnectionIntent);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateConnectionStatus();
        InvalidateConnectionIntent();
        InvalidateConnectionDetails();
        InvalidateFreeCountriesCount();
        InvalidateVpnPlan();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Subtitle));
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private Task ConnectAsync()
    {
        return _connectionManager.ConnectAsync(VpnTriggerDimension.ConnectionCard, CurrentConnectionIntent);
    }

    private bool CanConnect()
    {
        return IsDisconnected && _serviceManager.IsServiceEnabled;
    }

    // Shows a loading dialog for the whole enable attempt (a few seconds while Windows applies the
    // service config change and we poll for it - see ServiceEnabler.WaitUntilEnabledOrTimeoutAsync),
    // instead of leaving the button sitting there with no feedback. On failure, gives an actual reason
    // rather than just silently staying on the Enable button - most commonly the service isn't installed
    // at all (a dev-environment-only case; a real user hitting this has a broken/corrupt install), which
    // "start the service" alone can never fix, so it gets called out separately from a generic retry.
    [RelayCommand(CanExecute = nameof(ShowEnableServiceButton))]
    private async Task EnableServiceAsync()
    {
        Task<bool> enableTask = _serviceManager.EnableServiceAsync();

        await _mainWindowOverlayActivator.ShowLoadingMessageAsync(
            new MessageDialogParameters
            {
                Title = Localizer.Get("Dialogs_EnablingService_Title"),
                Message = Localizer.Get("Dialogs_EnablingService_Description"),
                ShowLoadingAnimation = true,
                CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
            },
            enableTask);

        if (!enableTask.IsCompleted)
        {
            // User dismissed the loading dialog before it resolved - let the attempt keep running in the
            // background; ServiceEnablementChangedMessage still updates the button once it truly settles.
            return;
        }

        bool succeeded = await enableTask;
        if (succeeded)
        {
            return;
        }

        await _mainWindowOverlayActivator.ShowMessageAsync(new MessageDialogParameters
        {
            Title = Localizer.Get("Dialogs_EnableServiceFailed_Title"),
            Message = _serviceManager.GetStatus() is null
                ? Localizer.Get("Dialogs_EnableServiceFailed_NotInstalled_Description")
                : Localizer.Get("Dialogs_EnableServiceFailed_Generic_Description"),
            CloseButtonText = Localizer.Get("Common_Actions_Close"),
        });
    }

    [RelayCommand(CanExecute = nameof(CanCancelConnection))]
    private Task CancelConnectionAsync()
    {
        return _connectionManager.DisconnectAsync(VpnTriggerDimension.ConnectionCard);
    }

    private bool CanCancelConnection()
    {
        return IsConnecting;
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private Task DisconnectAsync()
    {
        return _connectionManager.DisconnectAsync(VpnTriggerDimension.ConnectionCard);
    }

    private bool CanDisconnect()
    {
        return IsConnected;
    }

    [RelayCommand(CanExecute = nameof(CanShowFreeConnectionsOverlay))]
    private Task ShowFreeConnectionsOverlayAsync()
    {
        return _mainWindowOverlayActivator.ShowFreeConnectionsOverlayAsync();
    }

    private bool CanShowFreeConnectionsOverlay()
    {
        return IsDisconnected;
    }

    [RelayCommand]
    private Task ShowP2PInfoOverlayAsync()
    {
        return _mainWindowOverlayActivator.ShowP2PInfoOverlayAsync();
    }

    [RelayCommand]
    private Task ShowTorInfoOverlayAsync()
    {
        return _mainWindowOverlayActivator.ShowTorInfoOverlayAsync();
    }

    private void InvalidateConnectionStatus()
    {
        CurrentConnectionStatus = _connectionManager.ConnectionStatus;
    }

    private void InvalidateConnectionIntent()
    {
        IConnectionIntent? currentConnectionIntent = _connectionManager.CurrentConnectionIntent;

        CurrentConnectionIntent = _connectionManager.IsDisconnected || currentConnectionIntent == null
            ? _recentConnectionsManager.GetDefaultConnection()
            : currentConnectionIntent;

        // If intent is a profile, the reference might be the same even though profile details/settings have changed.
        // Force invalidating all properties.
        InvalidateAllProperties();
    }

    private void InvalidateConnectionDetails()
    {
        CurrentConnectionDetails = _connectionManager.CurrentConnectionDetails;
    }

    private void InvalidateFreeCountriesCount()
    {
        FreeCountriesCount = _serversLoader.GetFreeCountries().Count();
    }

    private void InvalidateVpnPlan()
    {
        OnPropertyChanged(nameof(IsFreeUser));
        OnPropertyChanged(nameof(IsFreeConnectionsTaglineVisible));
        OnPropertyChanged(nameof(IsChangeServerOptionVisible));
    }

    private string GetConnectionCardTitle()
    {
        return IsProfileIntent
            ? Profile!.Name
            : CurrentConnectionStatus switch
            {
                ConnectionStatus.Connected => Localizer.GetConnectionDetailsTitle(CurrentConnectionDetails),
                _ => Localizer.GetConnectionIntentTitle(CurrentConnectionIntent),
            };
    }

    private string GetConnectionCardSubtitle()
    {
        return CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected => IsProfileIntent
                ? Localizer.GetConnectionProfileDetailsSubtitle(CurrentConnectionDetails)
                : Localizer.GetConnectionDetailsSubtitle(CurrentConnectionDetails),
            _ => IsProfileIntent
                ? Localizer.GetConnectionProfileSubtitle(Profile)
                : Localizer.GetConnectionIntentSubtitle(CurrentConnectionIntent, useDetailedSubtitle: true),
        };
    }

    private bool IsFeature<TFeatureIntent>(ServerFeatures serverFeature)
        where TFeatureIntent : class, IFeatureIntent
    {
        return CurrentConnectionStatus switch
        {
            ConnectionStatus.Connected => CurrentConnectionDetails != null
                                       && CurrentConnectionDetails?.OriginalConnectionIntent.Feature is TFeatureIntent
                                       && CurrentConnectionDetails.Server.Features.IsSupported(serverFeature),
            _ => CurrentConnectionIntent?.Feature is TFeatureIntent
        };
    }
}