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

using Microsoft.UI.Xaml;
using SyncVPN.Client.Core.Messages;
using SyncVPN.Client.Core.Services.Selection;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Logic.Auth.Contracts.Messages;
using SyncVPN.Client.Settings.Contracts;
using SyncVPN.Client.Settings.Contracts.Messages;

namespace SyncVPN.Client.Services.Selection;

public class ApplicationThemeSelector : IApplicationThemeSelector,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<AuthenticationStatusChanged>
{
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;

    private readonly IList<ElementTheme> _themes;

    public ApplicationThemeSelector(
        ISettings settings,
        IEventMessageSender eventMessageSender)
    {
        _settings = settings;
        _eventMessageSender = eventMessageSender;

        _themes = Enum.GetValues<ElementTheme>();
    }

    public IList<ElementTheme> GetAvailableThemes()
    {
        return _themes;
    }

    public ElementTheme GetTheme()
    {
        return MapToElementTheme(string.IsNullOrEmpty(_settings.UserId)
            ? DefaultSettings.Theme
            : _settings.Theme);
    }

    public void SetTheme(ElementTheme theme)
    {
        _settings.Theme = theme.ToString();
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.Theme))
        {
            BroadcastThemeChange();
        }
    }

    public void Receive(AuthenticationStatusChanged message)
    {
        BroadcastThemeChange();
    }

    private static ElementTheme MapToElementTheme(string? theme)
    {
        if (Enum.TryParse(theme, out ElementTheme cacheTheme))
        {
            return cacheTheme;
        }

        return ElementTheme.Default;
    }

    private void BroadcastThemeChange()
    {
        _eventMessageSender.Send(new ThemeChangedMessage(GetTheme()));
    }
}