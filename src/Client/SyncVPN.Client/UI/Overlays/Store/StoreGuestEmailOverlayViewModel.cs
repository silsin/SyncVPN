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

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.Core.Bases;
using SyncVPN.Client.Core.Bases.ViewModels;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Common.Core.Extensions;

namespace SyncVPN.Client.UI.Overlays.Store;

// POST /checkout-links requires an email for a guest device (one with no logged-in account, so no
// email to extract server-side) - this is only ever invoked by StorePageViewModel when
// IUserAuthenticator.IsLoggedIn is false, right before creating a guest checkout link.
public partial class StoreGuestEmailOverlayViewModel : OverlayViewModelBase<IMainWindowOverlayActivator>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmailValid))]
    private string _email = string.Empty;

    public string Title => Localizer.Get("Store_GuestEmail_Title");

    public string Description => Localizer.Get("Store_GuestEmail_Description");

    public bool IsEmailValid => Email.IsValidEmailAddress();

    public StoreGuestEmailOverlayViewModel(
        IMainWindowOverlayActivator overlayActivator,
        IViewModelHelper viewModelHelper)
        : base(overlayActivator, viewModelHelper)
    {
    }

    // Returns the entered email, or null if the user cancelled.
    public async Task<string?> RequestEmailAsync()
    {
        Email = string.Empty;

        ContentDialogResult result = await InvokeAsync();

        return result == ContentDialogResult.Primary && IsEmailValid ? Email : null;
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Description));
    }
}
