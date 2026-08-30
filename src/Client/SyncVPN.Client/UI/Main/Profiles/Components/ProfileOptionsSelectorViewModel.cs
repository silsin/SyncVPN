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

using System.Security;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SyncVPN.Client.Common.Collections;
using SyncVPN.Client.Common.Enums;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Extensions;
using SyncVPN.Client.Core.Models;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.Factories;
using SyncVPN.Client.Logic.Profiles.Contracts;
using SyncVPN.Client.Logic.Profiles.Contracts.Models;
using SyncVPN.Client.Models.Profiles;
using SyncVPN.Client.UI.Main.Profiles.Contracts;
using SyncVPN.Common.Core.Extensions;

namespace SyncVPN.Client.UI.Main.Profiles.Components;

public partial class ProfileOptionsSelectorViewModel : ViewModelBase, IProfileOptionsSelector
{
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly ICommonItemFactory _commonItemFactory;

    private static readonly ConnectAndGoMode?[] _connectAndGoModes =
    {
        null,
        ConnectAndGoMode.Website,
        ConnectAndGoMode.Application
    };

    private IProfileOptions _originalProfileOptions = ProfileOptions.Default;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private ConnectAndGoModeItem? _selectedConnectAndGoMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectAndGoUrlValid))]
    [NotifyPropertyChangedFor(nameof(ConnectAndGoUrlErrorMessage))]
    private string _connectAndGoUrl = string.Empty;

    [ObservableProperty]
    private bool _usePrivateBrowsingMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectAndGoAppName))]
    private ExternalApp? _connectAndGoApp;

    public bool IsConnectAndGoUrlValid => string.IsNullOrEmpty(ConnectAndGoUrl) || ConnectAndGoUrl.IsValidUrl();

    public string ConnectAndGoUrlErrorMessage => !IsConnectAndGoUrlValid
        ? Localizer.Get("Profile_Options_Url_Error")
        : string.Empty;

    public string ConnectAndGoAppName => ConnectAndGoApp?.AppName ?? Localizer.Get("Profile_Options_Application_Select");

    public SmartObservableCollection<ConnectAndGoModeItem> ConnectAndGoModes { get; } = [];

    private bool ConnectAndGoStateHasChanged => _originalProfileOptions.ConnectAndGo.IsEnabled != SelectedConnectAndGoMode?.IsEnabled;

    private bool ConnectAndGoModeHasChanged => _originalProfileOptions.ConnectAndGo.Mode != SelectedConnectAndGoMode?.Mode;

    public ProfileOptionsSelectorViewModel(
        IMainWindowActivator mainWindowActivator,
        ICommonItemFactory commonItemFactory,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _mainWindowActivator = mainWindowActivator;
        _commonItemFactory = commonItemFactory;
    }

    public IProfileOptions GetProfileOptions()
    {
        // Turn off Connect and go on save if the URL or application is not valid
        bool isConnectAndGoEnabled = 
            SelectedConnectAndGoMode != null &&
            SelectedConnectAndGoMode.IsEnabled && 
            SelectedConnectAndGoMode.Mode switch
            {
                ConnectAndGoMode.Website => ConnectAndGoUrl.IsValidUrl(),
                ConnectAndGoMode.Application => ConnectAndGoApp?.IsValid ?? false,
                _ => false
            };

        return new ProfileOptions()
        {
            ConnectAndGo = new ConnectAndGoOption()
            {
                IsEnabled = isConnectAndGoEnabled,
                Mode = isConnectAndGoEnabled && SelectedConnectAndGoMode != null
                    ? SelectedConnectAndGoMode.Mode
                    : DefaultProfileSettings.ConnectAndGoMode,
                UsePrivateBrowsingMode = UsePrivateBrowsingMode,
                Url = ConnectAndGoUrl.Trim(),
                AppPath = ConnectAndGoApp?.AppPath
            }
        };
    }

    public async Task SetProfileOptionsAsync(IProfileOptions options)
    {
        _originalProfileOptions = options ?? ProfileOptions.Default;

        InvalidateCollections();

        SelectedConnectAndGoMode = _originalProfileOptions.ConnectAndGo.IsEnabled
            ? ConnectAndGoModes.FirstOrDefault(m => m.IsEnabled && _originalProfileOptions.ConnectAndGo.Mode == m.Mode)
            : ConnectAndGoModes.FirstOrDefault(m => !m.IsEnabled);
        UsePrivateBrowsingMode = _originalProfileOptions.ConnectAndGo.UsePrivateBrowsingMode;
        ConnectAndGoUrl = _originalProfileOptions.ConnectAndGo.Url;
        ConnectAndGoApp = await GetExternalAppAsync(_originalProfileOptions.ConnectAndGo.AppPath);
    }

    public bool HasChanged()
    {
        return ConnectAndGoStateHasChanged
            || ConnectAndGoModeHasChanged
            || SelectedConnectAndGoMode?.Mode switch
            {
                ConnectAndGoMode.Website =>
                    _originalProfileOptions.ConnectAndGo.Url != ConnectAndGoUrl.Trim() ||
                    _originalProfileOptions.ConnectAndGo.UsePrivateBrowsingMode != UsePrivateBrowsingMode,
                ConnectAndGoMode.Application =>
                    _originalProfileOptions.ConnectAndGo.AppPath != ConnectAndGoApp?.AppPath,
                _ => false
            };
    }

    public bool IsReconnectionRequired()
    {
        return false;
    }

    [RelayCommand]
    public async Task SelectAppAsync()
    {
        if (_mainWindowActivator.Window == null)
        {
            return;
        }

        string appFilePath = await _mainWindowActivator.Window.PickSingleFileAsync(Localizer.Get("Settings_Connection_SplitTunneling_Apps_FilesFilterName"), [ExternalApp.EXE_FILE_EXTENSION]);
        ConnectAndGoApp = await GetExternalAppAsync(appFilePath);
    }

    private void InvalidateCollections()
    {
        ConnectAndGoModes.Reset(_connectAndGoModes.Select(_commonItemFactory.GetConnectAndGoMode));
    }

    [RelayCommand]
    private void DisableConnectAndGo()
    {
        SelectConnectAndGoMode(false);
    }

    [RelayCommand]
    private void EnableConnectAndGoToWebsite()
    {
        SelectConnectAndGoMode(true, ConnectAndGoMode.Website);
    }

    [RelayCommand]
    private void EnableConnectAndGoToApplication()
    {
        SelectConnectAndGoMode(true, ConnectAndGoMode.Application);
    }

    private void SelectConnectAndGoMode(bool isEnabled, ConnectAndGoMode? mode = null)
    {
        SelectedConnectAndGoMode = isEnabled
            ? ConnectAndGoModes.FirstOrDefault(nsm => nsm.IsEnabled && nsm.Mode == mode)
            : ConnectAndGoModes.FirstOrDefault(nsm => !nsm.IsEnabled);
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(ConnectAndGoUrlErrorMessage));
        OnPropertyChanged(nameof(ConnectAndGoAppName));
    }

    private async Task<ExternalApp?> GetExternalAppAsync(string? appFilePath)
    {
        if (string.IsNullOrEmpty(appFilePath))
        {
            return null;
        }

        return await ExternalApp.TryCreateAsync(appFilePath)
            ?? ExternalApp.NotFound(appFilePath, Localizer.Get("Common_Message_AppNotFound"));
    }
}