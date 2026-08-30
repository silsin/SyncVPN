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
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Extensions;
using SyncVPN.Client.Logic.Connection.Contracts.History;
using SyncVPN.Client.Logic.Connection.Contracts.Messages;
using SyncVPN.Common.Core.Networking;

namespace SyncVPN.Client.UI.Main.Home.Details.Flyouts;

public partial class VolumeFlyoutViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<NetworkTrafficChangedMessage>
{
    private readonly INetworkTrafficManager _networkTrafficManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedDownloadVolume))]
    private long? _downloadVolume;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedUploadVolume))]
    private long? _uploadVolume;

    public string FormattedDownloadVolume => Localizer.GetFormattedSize(DownloadVolume);

    public string FormattedUploadVolume => Localizer.GetFormattedSize(UploadVolume);

    public VolumeFlyoutViewModel(
        INetworkTrafficManager networkTrafficManager,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _networkTrafficManager = networkTrafficManager;
    }

    public void Receive(NetworkTrafficChangedMessage message)
    {
        if (IsActive)
        {
            ExecuteOnUIThread(SetVolume);
        }
    }

    private void SetVolume()
    {
        NetworkTraffic volume = _networkTrafficManager.GetVolume();
        DownloadVolume = (long)volume.BytesDownloaded;
        UploadVolume = (long)volume.BytesUploaded;
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        SetVolume();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(FormattedDownloadVolume));
        OnPropertyChanged(nameof(FormattedUploadVolume));
    }
}