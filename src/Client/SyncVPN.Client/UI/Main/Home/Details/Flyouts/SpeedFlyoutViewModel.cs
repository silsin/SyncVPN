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

using CommunityToolkit.Mvvm.ComponentModel;
using SyncVPN.Client.Contracts.Services.Browsing;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts.History;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;
using SyncVPN.Common.Core.Networking;

namespace SyncVPN.Client.UI.Main.Home.Details.Flyouts;

public partial class SpeedFlyoutViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<NetworkTrafficChangedMessage>
{
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly ISettings _settings;
    private readonly INetworkTrafficManager _networkTrafficManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedDownloadSpeed))]
    private long? _downloadSpeed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedUploadSpeed))]
    private long? _uploadSpeed;

    public string FormattedDownloadSpeed => Localizer.GetFormattedSpeed(DownloadSpeed);

    public string FormattedUploadSpeed => Localizer.GetFormattedSpeed(UploadSpeed);

    public string UnderstandTrafficUri => _urlsBrowser.TrafficLearnMore;

    public SpeedFlyoutViewModel(
        IUrlsBrowser urlsBrowser,
        ISettings settings,
        INetworkTrafficManager networkTrafficManager,
        IViewModelHelper viewModelHelper) :
        base(viewModelHelper)
    {
        _urlsBrowser = urlsBrowser;
        _settings = settings;
        _networkTrafficManager = networkTrafficManager;
    }

    public void Receive(NetworkTrafficChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(SetSpeed);
        }
    }

    private void SetSpeed()
    {
        NetworkTraffic speed = _networkTrafficManager.GetSpeed();
        DownloadSpeed = (long)speed.BytesDownloaded;
        UploadSpeed = (long)speed.BytesUploaded;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        SetSpeed();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(FormattedDownloadSpeed));
        OnPropertyChanged(nameof(FormattedUploadSpeed));
    }
}