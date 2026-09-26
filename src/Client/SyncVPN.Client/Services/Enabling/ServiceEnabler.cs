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

using Microsoft.UI.Xaml.Controls;
using SyncVPN.Client.Common.Dispatching;
using SyncVPN.Client.Common.Models;
using SyncVPN.Client.Core.Services.Activation;
using SyncVPN.Client.EventMessaging.Contracts;
using SyncVPN.Client.Localization.Contracts;
using SyncVPN.Client.Logic.Services.Contracts;
using SyncVPN.Client.Logic.Services.Contracts.Messages;
using SyncVPN.Logging.Contracts;
using SyncVPN.Logging.Contracts.Events.AppServiceLogs;
using SyncVPN.OperatingSystems.Services.Contracts;

namespace SyncVPN.Client.Services.Enabling;

public class ServiceEnabler : IServiceEnabler
{
    private static readonly TimeSpan EnablePollInterval = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan EnablePollTimeout = TimeSpan.FromSeconds(5);

    private readonly ILogger _logger;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly ILocalizationProvider _localizer;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ServiceEnabler(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        ILocalizationProvider localizer,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        IEventMessageSender eventMessageSender)
    {
        _logger = logger;
        _uiThreadDispatcher = uiThreadDispatcher;
        _localizer = localizer;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _eventMessageSender = eventMessageSender;
    }

    public async Task EnableAsync(IService service, string? installPathIfMissing = null)
    {
        if (service.IsEnabled())
        {
            return;
        }

        await _semaphore.WaitAsync();

        try
        {
            await _uiThreadDispatcher.TryEnqueueAsync(() => ShowOverlayAsync(service, installPathIfMissing));
        }
        catch (Exception)
        {
            // The first call from bootstrapper calls this method too early and the UI is not yet ready.
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> TryEnableAsync(IService service, string? installPathIfMissing = null)
    {
        if (service.IsEnabled())
        {
            PublishState(service, isEnabled: true);
            return true;
        }

        await _semaphore.WaitAsync();

        try
        {
            _logger.Info<AppServiceLog>($"Attempting to enable service {service.Name}.");
            service.Enable(installPathIfMissing);
            await WaitUntilEnabledOrTimeoutAsync(service);

            bool isEnabled = service.IsEnabled();
            PublishState(service, isEnabled);
            return isEnabled;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ShowOverlayAsync(IService service, string? installPathIfMissing)
    {
        if (service.IsEnabled())
        {
            return;
        }

        MessageDialogParameters parameters = new()
        {
            Title = _localizer.GetFormat("Dialogs_DisabledService_Title"),
            Message = _localizer.Get("Dialogs_DisabledService_Description"),
            PrimaryButtonText = _localizer.Get("Common_Actions_Enable"),
            CloseButtonText = _localizer.Get("Common_Actions_Close"),
        };

        ContentDialogResult result = await _mainWindowOverlayActivator.ShowMessageAsync(parameters);
        if (result is ContentDialogResult.Primary)
        {
            _logger.Info<AppServiceLog>($"The user requested to enable service {service.Name}.");
            service.Enable(installPathIfMissing);
            await WaitUntilEnabledOrTimeoutAsync(service);
        }
        else
        {
            _logger.Info<AppServiceLog>($"The user refused to enable service {service.Name}.");
        }

        // Whatever the outcome, the app keeps running: a persistent banner (see
        // ServiceDisabledBannerViewModel) takes over from here and Connect stays disabled
        // until the service is actually enabled - the app no longer force-exits on failure.
        PublishState(service, service.IsEnabled());
    }

    private void PublishState(IService service, bool isEnabled)
    {
        _logger.Info<AppServiceLog>($"Service {service.Name} enablement state: {isEnabled}.");
        _eventMessageSender.Send(new ServiceEnablementChangedMessage(isEnabled));
    }

    // service.Enable() only fires off an elevated "sc config" call; the Windows Service Control
    // Manager can take a short moment to apply it, so poll instead of checking IsEnabled() once
    // immediately - otherwise a legitimately-succeeding enable can still be seen as a failure.
    private static async Task WaitUntilEnabledOrTimeoutAsync(IService service)
    {
        DateTime start = DateTime.UtcNow;
        while (!service.IsEnabled() && DateTime.UtcNow - start < EnablePollTimeout)
        {
            await Task.Delay(EnablePollInterval);
        }
    }
}
